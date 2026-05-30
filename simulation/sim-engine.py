import os
import json
import time

def load_json(path):
    if not os.path.exists(path):
        return None
    with open(path, 'r') as f:
        return json.load(f)

def run_simulation():
    print("====================================================")
    print("ESTATE VENUE COORDINATOR: Email Booking Simulation")
    print("====================================================")
    
    mock_inquiries = load_json("material-data/mock-inquiries.json")
    pricing_rules = load_json("material-data/venue-pricing-rules.json")
    
    if not mock_inquiries or not pricing_rules:
        print("[ERROR] Pricing rules or mock inquiries JSON could not be loaded!")
        return False
        
    print(f"[INFO] Loaded {len(mock_inquiries)} mock customer inquiries.")
    print(f"[INFO] VAT Rate configured: {pricing_rules.get('vatPercentage')}% ({pricing_rules.get('currency')})")
    
    print("\n--- STAGE 1: Public Availability Widget & Inbound Inquiry ---")
    inquiries_cache = []
    for idx, inquiry in enumerate(mock_inquiries, start=1):
        print(f"\n[INQUIRY #{idx}] From: {inquiry['senderName']} <{inquiry['senderEmail']}>")
        print(f"Subject: {inquiry['subject']}")
        print(f"Body snippet: \"{inquiry['body'][:90]}...\"")
        
        # Simulating Power Automate trigger
        print(">> [Power Automate] Triggered on new email in reservations@royalestate.com")
        time.sleep(0.3)
        print(f">> [Dynamics 365] Registering standard CRM Lead for customer '{inquiry['senderName']}'")
        lead_id = f"crm_lead_guid_{idx}"
        
        # Simulating Graph SendMail auto-reply
        print(f">> [M365 Graph] Dispatching branded HTML acknowledgement email to {inquiry['senderEmail']}")
        print("   Subject: 'Royal Estate Reservation Request Received'")
        print("   Attachment: Godset_Estate_Brochure.pdf")
        
        # Cache inquiry state
        inquiry['leadId'] = lead_id
        inquiry['state'] = "Inquiry"
        inquiries_cache.append(inquiry)
        time.sleep(0.2)
        
    print("\n--- STAGE 2: Coordinator Command Board Manual Review ---")
    for idx, card in enumerate(inquiries_cache, start=1):
        print(f"\n[BOARD CARD #{idx}] {card['senderName']} - {card['eventSubtype']} for {card['estimatedGuestCount']} guests")
        print(f"Assigned Area: {card['targetArea']}")
        print(">> Coordinator drags card from 'Inquiries' -> 'Confirmed Bookings'")
        time.sleep(0.4)
        
        # Determine packages and calculate pricing
        pkg = None
        for p in pricing_rules.get("packages", []):
            if card['eventSubtype'] in p['name'] or card['targetArea'] in p['description']:
                pkg = p
                break
        if not pkg:
            pkg = pricing_rules.get("packages", [])[0]
            
        unit_net = pkg['basePrice']
        vat_sum = unit_net * (pricing_rules['vatPercentage'] / 100.0)
        gross_sum = unit_net + vat_sum
        
        print(f"[e-conomic ERP API] Executing 100% complete invoicing integration...")
        print(f">> lookup customer '{card['senderName']}' by email '{card['senderEmail']}'")
        cust_no = 20000 + idx
        print(f">> Resolved customer profile: customerNumber = {cust_no}")
        
        print(f">> POST /invoices/drafts: Product: {pkg['productNumber']}, Net Price: DKK {unit_net:,.2f}")
        draft_no = 4000 + idx
        print(f">> Draft Invoice created successfully: draftInvoiceNumber = {draft_no}")
        
        print(f">> POST /invoices/booked: Booking draft {draft_no} to finalize transaction...")
        booked_no = 80000 + idx
        pay_link = f"https://payment.e-conomic.com/invoice/{booked_no}/pay?token=sim_pay_token_{idx}"
        print(f">> Invoice booked successfully: BookedInvoiceNumber = {booked_no}")
        print(f">> Direct transaction PDF / Paylink: {pay_link}")
        
        # Final contract email trigger
        print(f"[M365 Graph] Dispatching Wedding/Birthday Contract PDF + e-conomic Invoice link...")
        print(f">> Email sent to {card['senderEmail']} containing contract attachment and payment link.")
        time.sleep(0.3)
        
    print("\n====================================================")
    print("[SUCCESS] Full Booking Scheduling & e-conomic Invoicing Simulation Complete!")
    print("====================================================")
    return True

if __name__ == "__main__":
    run_simulation()
