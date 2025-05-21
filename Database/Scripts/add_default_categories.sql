DELETE FROM "CATEGORY"
WHERE user_id = -1;

INSERT INTO "CATEGORY" (name, user_id)
VALUES
    ('Electronics', -1),
    ('Appliances', -1),
    ('Consumables', -1),
    ('Hygiene', -1),
    ('Furniture', -1),
    ('Games', -1),
    ('Culinary', -1),
    ('Tools', -1),
    ('Garden', -1),
    ('Misc', -1),
    ('Laundry', -1),
    ('Lighting', -1),
    ('Decor', -1),
    ('Office Supplies', -1),
    ('Pet Supplies', -1),
    ('Cleaning Supplies', -1),
    ('Bedding', -1),
    ('Storage & Organization', -1),
    ('Bath', -1),
    ('Safety & Security', -1),
    ('Maintenance', -1),
    ('Seasonal Items', -1),
    ('Entertainment', -1),
    ('Hardware', -1),
    ('Automotive', -1);