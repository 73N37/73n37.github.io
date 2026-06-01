-- ============================================================
-- AIDA Estate Command — Local PostgreSQL/PostgREST Table Schema
-- ============================================================

-- 1. Create the events table
CREATE TABLE IF NOT EXISTS events (
    id                           VARCHAR(100) PRIMARY KEY,
    dynamics_entity_id          VARCHAR(100),
    dynamics_entity_logical_name VARCHAR(100),
    metadata_id                  UUID,
    subject                      VARCHAR(200) NOT NULL,
    body_content                 TEXT,
    section_key                  VARCHAR(50) NOT NULL DEFAULT 'inquiry',
    start_utc                    TIMESTAMPTZ NOT NULL,
    end_utc                      TIMESTAMPTZ NOT NULL,
    color_hex                    VARCHAR(10),
    priority                     VARCHAR(20),
    smart_summary                TEXT,
    action_items                 JSONB NOT NULL DEFAULT '[]'::JSONB,
    subevents                    JSONB NOT NULL DEFAULT '[]'::JSONB,
    event_subtype                VARCHAR(50),
    guest_count                  INT NOT NULL DEFAULT 0,
    assigned_coordinator         VARCHAR(100),
    estate_area                  VARCHAR(100),
    catering_option              VARCHAR(100),
    price                        NUMERIC,
    economic_invoice_number      INT,
    economic_payment_link        TEXT,
    created_at                   TIMESTAMPTZ DEFAULT NOW()
);

-- 2. Seed with the three demo bookings so the board shows data immediately
INSERT INTO events (
    id, subject, body_content, section_key,
    start_utc, end_utc, color_hex, priority,
    event_subtype, guest_count, assigned_coordinator,
    estate_area, catering_option, economic_invoice_number,
    economic_payment_link, subevents
) VALUES
(
    'demo-event-001',
    'Kensington Bryllup i Den Store Lade',
    'Et eksklusivt gods-bryllup med fuld forplejning, blomsterdekorationer i Den Store Lade og velkomstchampagne i Søparken.',
    'confirmed',
    NOW()::date + interval '11 hours',
    NOW()::date + interval '18 hours',
    '#D4AF37', 'High', 'Bryllup', 120, 'Sarah Jenkins',
    'Den Store Lade', 'Gourmet Selskabsmenu',
    104052, 'https://payment.e-conomic.com/invoice/104052/pay?token=demo_token_johnson',
    '[
        {"id":"00000000-0000-0000-0000-000000000001","title":"Velkomstreception & Champagne","startTime":"11:00:00","endTime":"12:30:00","location":"Søparken"},
        {"id":"00000000-0000-0000-0000-000000000002","title":"Bryllupsmiddag & Taler","startTime":"13:00:00","endTime":"17:00:00","location":"Den Store Lade"},
        {"id":"00000000-0000-0000-0000-000000000003","title":"Brudevals & Kageskæring","startTime":"17:15:00","endTime":"18:00:00","location":"Den Store Lade"}
    ]'::jsonb
),
(
    'demo-event-002',
    'Sterling Konference i Hovedbygningen',
    'Dagsmøde og konference i Hovedbygningen for Manor Holdings. Kræver AV-opsætning og konference-dagsmenu.',
    'preparation',
    NOW()::date + interval '8 hours',
    NOW()::date + interval '14 hours',
    '#10B981', 'Normal', 'Konference', 80, 'Michael Chang',
    'Hovedbygningen', 'Konference-dagsmenu',
    NULL, NULL,
    '[
        {"id":"00000000-0000-0000-0000-000000000004","title":"Morgenmad & Netværk","startTime":"08:00:00","endTime":"09:00:00","location":"Hovedbygningen"},
        {"id":"00000000-0000-0000-0000-000000000005","title":"Formiddagssession & Keynote","startTime":"09:00:00","endTime":"12:00:00","location":"Hovedbygningen"},
        {"id":"00000000-0000-0000-0000-000000000006","title":"Forretningsfrokost","startTime":"12:00:00","endTime":"13:00:00","location":"Hovedbygningen"}
    ]'::jsonb
),
(
    'demo-event-003',
    'Midsommerfest i Søparken Henvendelse',
    'Forespørgsel på leje af herregårdshaven (Søparken) og Hovedbygningen. Kræver formelt tilbud.',
    'inquiry',
    NOW()::date + interval '2 days' + interval '9 hours',
    NOW()::date + interval '2 days' + interval '15 hours',
    '#D4AF37', 'High', 'Bryllup', 200, 'Sarah Jenkins',
    'Søparken', 'Brunch & Champagne',
    NULL, NULL,
    '[
        {"id":"00000000-0000-0000-0000-000000000007","title":"Velkomst & Kaffe","startTime":"10:00:00","endTime":"11:00:00","location":"Søparken"},
        {"id":"00000000-0000-0000-0000-000000000008","title":"Have-reception & Champagne","startTime":"11:00:00","endTime":"14:00:00","location":"Søparken"},
        {"id":"00000000-0000-0000-0000-000000000009","title":"Brunch & Networking","startTime":"14:00:00","endTime":"16:00:00","location":"Søparken"}
    ]'::jsonb
)
ON CONFLICT (id) DO NOTHING;
