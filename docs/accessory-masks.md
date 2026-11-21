# Paper-mask head accessories (no asset bundles)

SPRK can add head accessories shaped like **Haphazardly Drawn Mask of Goblin**
(a thin, curved paper cut-out) from any image with a transparent background,
without building a Unity asset bundle. The first one is **Mask of xdd**
(accessory and item 3102599), rewarded by the gift code `PDFABERGIFT2`.

## How it works

| Piece | Where |
| --- | --- |
| Model and art generator | `client/tools/AccessoryMasks/make_paper_mask.py` (source image `client/tools/AccessoryMasks/xdd.png`) |
| Mesh + texture (embedded) | `client/managed/SprkAccessories/SprkMasks.<Name>.mesh` / `.png` |
| Runtime swap | `client/managed/SprkAccessories/SprkAccessoryMasks.cs` |
| Item icon / Dressing Room image (embedded) | `client/managed/SprkImageRecovery/SprkIcons.Item.<CODE>.png`, `SprkPortraits.<name>.png`, mapped in `SprkImageRecovery.bundleImages` |
| Client tables and text | `client/tables/costumes/30-33-xdd-mask-*.json` |
| Server tables | `server/tables/ExtensionSupport.json` (`AccessoryCostume` with `"SprkGiftOnly": true`, `AccessorySelectItem`), `HeroShopSupport.json`, `InventorySupport.json`, `ItemCodeToIndex.json`, `ItemRewardMetadata.json`, `CouponPool.json` |

- The accessory code (`Accessory.Head.Head.SprkXdd`) has no effect of its own.
  `BuilderContainer.Get` loads the Goblin mask's effect instead
  (`SprkAccessoryMasks.EffectCode`), so the mask keeps the Goblin's pivot,
  shader and per-hero placement.
- `EffectLoad` tags the effect instance with the requested code. Right after
  `ModelInstance` takes the Goblin model from its pool, before `RenderProxy`
  captures the materials, `SprkAccessoryMasks.Apply` swaps in the embedded mesh
  and a copy of the material with the embedded texture. Before the model goes
  back to the pool, `Restore` puts the Goblin mesh and material back.
- The generator traces the image's alpha outline, adds a white paper border,
  and builds a front face (the image), a white back and dark side walls. They
  use the Goblin mask's width, height, curvature (`y = 3.41·x² − 0.131`) and
  thickness (0.0091). `--goblin <bundle>` re-measures those values from the
  original mesh.

## Gift-only accessories

SPRK sells every accessory in the Dressing Room. A gift-only accessory is
listed in `AccessoryCostumeData.IsSprkGiftOnly`, which makes `IsBuy` false
and `PreviewableWhenOwned` true. It is then hidden from the shop, and the
Dressing Room shows it only for heroes that own it. The server refuses to sell
it (`AccessoryCostumeNotOpened`) when its `ExtensionSupport` row has
`"SprkGiftOnly": true`. Players get it as an item (`AccessorySelectItem`)
and apply it to a hero, like the Goblin mask item.

## Adding another mask

1. Run `python client/tools/AccessoryMasks/make_paper_mask.py <image.png> <Name> <ITEM_CODE> <Portrait_Name>`.
2. Add `"Accessory.Head.Head.Sprk<Name>"` to `SprkAccessoryMasks.masks`, and the
   two UI paths to `SprkImageRecovery.bundleImages`.
3. Add client table rows and text like `client/tables/costumes/30-33-*`, then run
   `python client/scripts/build/build_tables.py`.
4. Add the server rows like 3102599. For a gift-only mask, also add its index to
   `IsSprkGiftOnly` and `"SprkGiftOnly": true` to its server row.
5. Check it in the game with the `accessory-mask-probe` automation command.
   It needs the Login scene and an `accessories-*` profile; its `text` is a
   report path, and it writes `<path>.png` and `<path>.wide.png`.
