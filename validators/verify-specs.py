import os
import json
import sys

def main():
    print("====================================================")
    print("Spec-Driven Development Validator: Verifying Specs")
    print("====================================================")
    
    config_path = ".spec-config.json"
    if not os.path.exists(config_path):
        print(f"[ERROR] Configuration file '{config_path}' not found at workspace root!")
        sys.exit(1)
        
    try:
        with open(config_path, 'r') as f:
            config = json.load(f)
    except Exception as e:
        print(f"[ERROR] Failed to parse JSON config: {e}")
        sys.exit(1)
        
    specs_dir = config.get("specifications", {}).get("directory", "specs")
    required_specs = config.get("specifications", {}).get("requiredSpecs", [])
    
    print(f"[INFO] Checking specifications directory: '{specs_dir}'")
    if not os.path.exists(specs_dir):
        print(f"[ERROR] Specifications directory '{specs_dir}' is missing!")
        sys.exit(1)
        
    failed = False
    for spec in required_specs:
        spec_path = os.path.join(specs_dir, spec)
        if os.path.exists(spec_path):
            print(f"[SUCCESS] Spec verified: {spec_path}")
        else:
            print(f"[ERROR] Required spec is missing: {spec_path}")
            failed = True
            
    if failed:
        print("[ERROR] SDD Validation FAILED due to missing specification files.")
        sys.exit(1)
    else:
        print("[SUCCESS] SDD Validation PASSED. All required specifications are intact.")
        print("====================================================")
        sys.exit(0)

if __name__ == "__main__":
    main()
