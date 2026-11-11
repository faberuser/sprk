//! SQLite-compatible row decoding on PostgreSQL.
//!
//! SQLite stores integers as one 64-bit type and sqlx's SQLite driver decodes
//! them into any Rust integer or `bool`. sqlx's PostgreSQL driver is strict:
//! `i32` only from `integer`, `bool` only from `boolean`, `i64` only from
//! `bigint`. The game code decodes `bigint` columns into `i32`, `u8` and `bool`
//! fields, and SQL integer literals (`SELECT 1`) arrive as `integer`.
//!
//! `query_scalar` / `query_as` here are drop-in replacements for the sqlx
//! functions, and `RowExt` replaces `sqlx::Row::get` / `try_get`. They decode
//! any integer type into any integer type (range-checked), integers into
//! `bool` (non-zero) and `f64`, and JSON text into `serde_json::Value`.
use sqlx::{
    postgres::{PgArguments, PgRow, PgValueRef},
    ColumnIndex, Encode, Error, Executor, Postgres, Row, Type, TypeInfo, ValueRef,
};
use std::{fmt::Debug, marker::PhantomData};

fn decode_error<I: Debug>(index: I, message: String) -> Error {
    Error::ColumnDecode { index: format!("{index:?}"), source: message.into() }
}

fn raw<I: ColumnIndex<PgRow> + Debug + Copy>(row: &PgRow, index: I) -> Result<Option<PgValueRef<'_>>, Error> {
    let value = row.try_get_raw(index)?;
    Ok(if value.is_null() { None } else { Some(value) })
}

/// A Rust value decodable from one column with SQLite's leniency.
pub(crate) trait Column: Sized {
    fn column<I: ColumnIndex<PgRow> + Debug + Copy>(row: &PgRow, index: I) -> Result<Self, Error>;
}

/// The column as a 64-bit integer, whatever its integer width.
fn int<I: ColumnIndex<PgRow> + Debug + Copy>(row: &PgRow, index: I) -> Result<Option<i64>, Error> {
    let Some(value) = raw(row, index)? else { return Ok(None) };
    let kind = value.type_info().name().to_owned();
    Ok(Some(match kind.as_str() {
        "INT8" => Row::try_get::<i64, _>(row, index)?,
        "INT4" => Row::try_get::<i32, _>(row, index)?.into(),
        "INT2" => Row::try_get::<i16, _>(row, index)?.into(),
        "BOOL" => Row::try_get::<bool, _>(row, index)?.into(),
        other => return Err(decode_error(index, format!(
            "expected an integer column, found {other} (cast it to bigint in SQL)"))),
    }))
}

fn required<T, I: Debug>(index: I, value: Option<T>) -> Result<T, Error> {
    value.ok_or_else(|| Error::ColumnDecode { index: format!("{index:?}"), source: "unexpected NULL".into() })
}

macro_rules! integer_column {
    ($($t:ty),*) => {$(
        impl Column for $t {
            fn column<I: ColumnIndex<PgRow> + Debug + Copy>(row: &PgRow, index: I) -> Result<Self, Error> {
                let value = required(index, int(row, index)?)?;
                <$t>::try_from(value).map_err(|_| decode_error(index, format!("{value} is out of range for {}", stringify!($t))))
            }
        }
    )*};
}
integer_column!(i64, i32, i16, i8, u64, u32, u16, u8, usize, isize);

impl Column for bool {
    fn column<I: ColumnIndex<PgRow> + Debug + Copy>(row: &PgRow, index: I) -> Result<Self, Error> {
        Ok(required(index, int(row, index)?)? != 0)
    }
}

impl Column for f64 {
    fn column<I: ColumnIndex<PgRow> + Debug + Copy>(row: &PgRow, index: I) -> Result<Self, Error> {
        let value = required(index, raw(row, index)?)?;
        match value.type_info().name() {
            "FLOAT8" => Row::try_get::<f64, _>(row, index),
            "FLOAT4" => Row::try_get::<f32, _>(row, index).map(f64::from),
            _ => Ok(required(index, int(row, index)?)? as f64),
        }
    }
}

impl Column for f32 {
    fn column<I: ColumnIndex<PgRow> + Debug + Copy>(row: &PgRow, index: I) -> Result<Self, Error> {
        f64::column(row, index).map(|v| v as f32)
    }
}

impl Column for String {
    fn column<I: ColumnIndex<PgRow> + Debug + Copy>(row: &PgRow, index: I) -> Result<Self, Error> {
        let value = required(index, raw(row, index)?)?;
        match value.type_info().name() {
            "JSON" | "JSONB" => Row::try_get::<serde_json::Value, _>(row, index).map(|v| v.to_string()),
            _ => Row::try_get::<String, _>(row, index),
        }
    }
}

impl Column for serde_json::Value {
    fn column<I: ColumnIndex<PgRow> + Debug + Copy>(row: &PgRow, index: I) -> Result<Self, Error> {
        let value = required(index, raw(row, index)?)?;
        match value.type_info().name() {
            "JSON" | "JSONB" => Row::try_get::<serde_json::Value, _>(row, index),
            _ => {
                let text = Row::try_get::<String, _>(row, index)?;
                serde_json::from_str(&text).map_err(|e| decode_error(index, format!("invalid JSON: {e}")))
            }
        }
    }
}

impl Column for Vec<u8> {
    fn column<I: ColumnIndex<PgRow> + Debug + Copy>(row: &PgRow, index: I) -> Result<Self, Error> {
        Row::try_get::<Vec<u8>, _>(row, index)
    }
}

impl<T: Column> Column for Option<T> {
    fn column<I: ColumnIndex<PgRow> + Debug + Copy>(row: &PgRow, index: I) -> Result<Self, Error> {
        match raw(row, index)? {
            None => Ok(None),
            Some(_) => T::column(row, index).map(Some),
        }
    }
}

/// A whole row decodable into a tuple, column by column.
pub(crate) trait Columns: Sized {
    fn columns(row: &PgRow) -> Result<Self, Error>;
}

macro_rules! tuple_columns {
    ($($t:ident $i:tt),+) => {
        impl<$($t: Column),+> Columns for ($($t,)+) {
            fn columns(row: &PgRow) -> Result<Self, Error> {
                Ok(($($t::column(row, $i as usize)?,)+))
            }
        }
    };
}
tuple_columns!(A 0);
tuple_columns!(A 0, B 1);
tuple_columns!(A 0, B 1, C 2);
tuple_columns!(A 0, B 1, C 2, D 3);
tuple_columns!(A 0, B 1, C 2, D 3, E 4);
tuple_columns!(A 0, B 1, C 2, D 3, E 4, F 5);
tuple_columns!(A 0, B 1, C 2, D 3, E 4, F 5, G 6);
tuple_columns!(A 0, B 1, C 2, D 3, E 4, F 5, G 6, H 7);
tuple_columns!(A 0, B 1, C 2, D 3, E 4, F 5, G 6, H 7, J 8);
tuple_columns!(A 0, B 1, C 2, D 3, E 4, F 5, G 6, H 7, J 8, K 9);
tuple_columns!(A 0, B 1, C 2, D 3, E 4, F 5, G 6, H 7, J 8, K 9, L 10);
tuple_columns!(A 0, B 1, C 2, D 3, E 4, F 5, G 6, H 7, J 8, K 9, L 10, M 11);
tuple_columns!(A 0, B 1, C 2, D 3, E 4, F 5, G 6, H 7, J 8, K 9, L 10, M 11, N 12);
tuple_columns!(A 0, B 1, C 2, D 3, E 4, F 5, G 6, H 7, J 8, K 9, L 10, M 11, N 12, P 13);
tuple_columns!(A 0, B 1, C 2, D 3, E 4, F 5, G 6, H 7, J 8, K 9, L 10, M 11, N 12, P 13, Q 14);
tuple_columns!(A 0, B 1, C 2, D 3, E 4, F 5, G 6, H 7, J 8, K 9, L 10, M 11, N 12, P 13, Q 14, R 15);

/// `row.get(..)` / `row.try_get(..)` with SQLite's leniency. Import this
/// instead of `sqlx::Row`.
pub(crate) trait RowExt {
    fn get<T: Column, I: ColumnIndex<PgRow> + Debug + Copy>(&self, index: I) -> T;
    fn try_get<T: Column, I: ColumnIndex<PgRow> + Debug + Copy>(&self, index: I) -> Result<T, Error>;
}

impl RowExt for PgRow {
    #[track_caller]
    fn get<T: Column, I: ColumnIndex<PgRow> + Debug + Copy>(&self, index: I) -> T {
        T::column(self, index).unwrap_or_else(|e| panic!("{e}"))
    }
    fn try_get<T: Column, I: ColumnIndex<PgRow> + Debug + Copy>(&self, index: I) -> Result<T, Error> {
        T::column(self, index)
    }
}

/// How a mapped query turns a row into `O`.
pub(crate) trait Mapping<O> {
    fn map(row: &PgRow) -> Result<O, Error>;
}
pub(crate) struct Scalar;
pub(crate) struct Tuple;
impl<O: Column> Mapping<O> for Scalar {
    fn map(row: &PgRow) -> Result<O, Error> { O::column(row, 0usize) }
}
impl<O: Columns> Mapping<O> for Tuple {
    fn map(row: &PgRow) -> Result<O, Error> { O::columns(row) }
}

/// A query whose rows are decoded with `M`. Mirrors the sqlx builder API.
#[must_use = "query must be executed to affect database"]
pub(crate) struct Mapped<O, M> {
    inner: sqlx::query::Query<'static, Postgres, PgArguments>,
    _out: PhantomData<fn() -> (O, M)>,
}

/// Drop-in for `sqlx::query_scalar`: decodes the first column.
pub(crate) fn query_scalar<O: Column>(sql: impl sqlx::SqlSafeStr) -> Mapped<O, Scalar> {
    Mapped { inner: sqlx::query(sql), _out: PhantomData }
}

/// Drop-in for `sqlx::query_as` with tuple outputs.
pub(crate) fn query_as<O: Columns>(sql: impl sqlx::SqlSafeStr) -> Mapped<O, Tuple> {
    Mapped { inner: sqlx::query(sql), _out: PhantomData }
}

impl<O: Send + Unpin + 'static, M: Mapping<O>> Mapped<O, M> {
    pub(crate) fn bind<'t, T: Bind<'t>>(mut self, value: T) -> Self {
        self.inner = self.inner.bind(value.into_bind());
        self
    }

    pub(crate) async fn fetch_all<'e, 'c: 'e, E>(self, executor: E) -> Result<Vec<O>, Error>
    where E: 'e + Executor<'c, Database = Postgres> {
        self.inner.try_map(|row: PgRow| M::map(&row)).fetch_all(executor).await
    }

    pub(crate) async fn fetch_one<'e, 'c: 'e, E>(self, executor: E) -> Result<O, Error>
    where E: 'e + Executor<'c, Database = Postgres> {
        self.inner.try_map(|row: PgRow| M::map(&row)).fetch_one(executor).await
    }

    pub(crate) async fn fetch_optional<'e, 'c: 'e, E>(self, executor: E) -> Result<Option<O>, Error>
    where E: 'e + Executor<'c, Database = Postgres> {
        self.inner.try_map(|row: PgRow| M::map(&row)).fetch_optional(executor).await
    }
}

/// A value bound the way SQLite stored it. Every integer width and `bool`
/// binds as `bigint` (SQLite had one integer type and stored booleans as
/// 0/1), and JSON values bind as text. This also keeps one parameter type
/// per SQL text: sqlx caches prepared statements by SQL, and two call sites
/// binding `i32` and `i64` to the same statement would otherwise fail with
/// "incorrect binary data format".
pub(crate) trait Bind<'a> {
    type Out: Encode<'a, Postgres> + Type<Postgres> + 'a;
    fn into_bind(self) -> Self::Out;
}

macro_rules! bind_int {
    ($($t:ty),*) => {$(
        impl<'a> Bind<'a> for $t {
            type Out = i64;
            fn into_bind(self) -> i64 { i64::try_from(self).unwrap_or(i64::MAX) }
        }
        impl<'a> Bind<'a> for &'a $t {
            type Out = i64;
            fn into_bind(self) -> i64 { i64::try_from(*self).unwrap_or(i64::MAX) }
        }
        impl<'a> Bind<'a> for Option<$t> {
            type Out = Option<i64>;
            fn into_bind(self) -> Option<i64> { self.map(|v| i64::try_from(v).unwrap_or(i64::MAX)) }
        }
        impl<'a> Bind<'a> for &'a Option<$t> {
            type Out = Option<i64>;
            fn into_bind(self) -> Option<i64> { self.map(|v| i64::try_from(v).unwrap_or(i64::MAX)) }
        }
    )*};
}
bind_int!(i64, i32, i16, i8, u64, u32, u16, u8, usize, isize);

impl<'a> Bind<'a> for bool {
    type Out = i64;
    fn into_bind(self) -> i64 { i64::from(self) }
}
impl<'a> Bind<'a> for &'a bool {
    type Out = i64;
    fn into_bind(self) -> i64 { i64::from(*self) }
}
impl<'a> Bind<'a> for Option<bool> {
    type Out = Option<i64>;
    fn into_bind(self) -> Option<i64> { self.map(i64::from) }
}

macro_rules! bind_float {
    ($($t:ty),*) => {$(
        impl<'a> Bind<'a> for $t {
            type Out = f64;
            fn into_bind(self) -> f64 { f64::from(self) }
        }
        impl<'a> Bind<'a> for &'a $t {
            type Out = f64;
            fn into_bind(self) -> f64 { f64::from(*self) }
        }
    )*};
}
bind_float!(f64, f32);

macro_rules! bind_as_is {
    ($($t:ty),*) => {$(
        impl<'a> Bind<'a> for $t {
            type Out = $t;
            fn into_bind(self) -> $t { self }
        }
    )*};
}
bind_as_is!(String, Option<String>, &'a str, Option<&'a str>, Vec<u8>, &'a [u8]);

impl<'a> Bind<'a> for &'a String {
    type Out = &'a str;
    fn into_bind(self) -> &'a str { self.as_str() }
}
impl<'a> Bind<'a> for &'a &'a str {
    type Out = &'a str;
    fn into_bind(self) -> &'a str { self }
}
impl<'a> Bind<'a> for &'a Option<String> {
    type Out = Option<&'a str>;
    fn into_bind(self) -> Option<&'a str> { self.as_deref() }
}
impl<'a> Bind<'a> for &'a Vec<u8> {
    type Out = &'a [u8];
    fn into_bind(self) -> &'a [u8] { self.as_slice() }
}
impl<'a> Bind<'a> for serde_json::Value {
    type Out = String;
    fn into_bind(self) -> String { self.to_string() }
}
impl<'a> Bind<'a> for &'a serde_json::Value {
    type Out = String;
    fn into_bind(self) -> String { self.to_string() }
}

/// Drop-in for `sqlx::query` with SQLite-compatible binds (see `Bind`).
#[must_use = "query must be executed to affect database"]
pub(crate) struct Query {
    inner: sqlx::query::Query<'static, Postgres, PgArguments>,
}

pub(crate) fn query(sql: impl sqlx::SqlSafeStr) -> Query {
    Query { inner: sqlx::query(sql) }
}

impl Query {
    pub(crate) fn bind<'t, T: Bind<'t>>(mut self, value: T) -> Self {
        self.inner = self.inner.bind(value.into_bind());
        self
    }

    pub(crate) async fn execute<'e, 'c: 'e, E>(self, executor: E) -> Result<sqlx::postgres::PgQueryResult, Error>
    where E: 'e + Executor<'c, Database = Postgres> {
        self.inner.execute(executor).await
    }

    pub(crate) async fn fetch_all<'e, 'c: 'e, E>(self, executor: E) -> Result<Vec<PgRow>, Error>
    where E: 'e + Executor<'c, Database = Postgres> {
        self.inner.fetch_all(executor).await
    }

    pub(crate) async fn fetch_one<'e, 'c: 'e, E>(self, executor: E) -> Result<PgRow, Error>
    where E: 'e + Executor<'c, Database = Postgres> {
        self.inner.fetch_one(executor).await
    }

    pub(crate) async fn fetch_optional<'e, 'c: 'e, E>(self, executor: E) -> Result<Option<PgRow>, Error>
    where E: 'e + Executor<'c, Database = Postgres> {
        self.inner.fetch_optional(executor).await
    }
}
