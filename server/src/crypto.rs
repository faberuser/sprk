use rand::RngExt;

/// Generate a random session key
pub fn generate_session_key() -> String {
    let mut rng = rand::rng();
    let bytes: [u8; 32] = rng.random();
    hex::encode(bytes)
}

/// Generate a random AES key
pub fn generate_aes_key() -> String {
    let mut rng = rand::rng();
    let bytes: [u8; 16] = rng.random();
    hex::encode(bytes)
}
