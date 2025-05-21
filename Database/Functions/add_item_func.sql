CREATE OR REPLACE FUNCTION add_item(
    p_name VARCHAR DEFAULT 'ERR_EMPTY',
    p_description VARCHAR DEFAULT NULL,
    p_user_id INTEGER DEFAULT -1,
    p_barcode VARCHAR DEFAULT NULL,
    p_category_id INTEGER DEFAULT -1,
    p_location_id INTEGER DEFAULT -1,
    p_photo_id INTEGER DEFAULT -1,
    p_receipt_id INTEGER DEFAULT -1
)
RETURNS BIGINT AS $$
DECLARE
    new_item_id BIGINT;
BEGIN
    INSERT INTO "ITEM" (
        name, description, user_id, barcode,
        category_id, location_id, photo_id, receipt_id
    )
    VALUES (
        p_name, p_description, p_user_id, p_barcode,
        p_category_id, p_location_id, p_photo_id, p_receipt_id
    )
    RETURNING id INTO new_item_id;

    RETURN new_item_id;
END;
$$ LANGUAGE plpgsql;


