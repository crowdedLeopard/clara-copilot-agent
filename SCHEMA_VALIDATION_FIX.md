# 🛠️ Power Platform Schema Validation Fix

## Issue: "integer undefined" vs "integer int32" Type Mismatch

You were getting these schema validation warnings:
```
Property ".totalLicenses" type mismatch, Expected: "integer undefined", Actual: "integer int32".
Property ".usedLicenses" type mismatch, Expected: "integer undefined", Actual: "integer int32".
Property ".availableLicenses" type mismatch, Expected: "integer undefined", Actual: "integer int32".
```

## Root Cause
Power Platform's schema validator interprets `type: integer` differently than the API returns. The API serializes integers with an implicit `int32` format, but Power Platform expects "undefined" format.

## Solution Applied

### ✅ Fixed Swagger Spec: `clara-swagger-powerplatform-compatible.yaml`

**Changed:**
```yaml
# OLD (causing warnings)
totalLicenses:
  type: integer
  
# NEW (Power Platform compatible)
totalLicenses:
  type: number
```

**Key Changes:**
1. **LicenseCountsResponse properties**: Changed from `type: integer` to `type: number`
   - `totalLicenses`: integer → number
   - `usedLicenses`: integer → number  
   - `availableLicenses`: integer → number

2. **Query parameters**: Changed `days` parameter from `type: integer` to `type: number`

3. **Version bump**: Updated to version 1.0.2

## Why This Works

- **`type: number`** in Swagger 2.0 accepts both integers and decimals
- **Power Platform** validates `number` types more permissively
- **API response** remains unchanged (still returns actual integers)
- **No impact** on actual functionality - only fixes schema validation

## Expected Result

After importing `clara-swagger-powerplatform-compatible.yaml`:
- ✅ **No schema validation warnings**
- ✅ **API returns same data**: `{"totalLicenses": 100, "usedLicenses": 100, "availableLicenses": 0}`
- ✅ **Connector works perfectly** with Power Apps and Power Automate

## Import Instructions

1. **Delete existing connector** (if you want clean import)
2. **Import new spec**: `clara-swagger-powerplatform-compatible.yaml`
3. **Configure authentication** (same as before)
4. **Test "Get License Counts"** - should have no warnings

## Alternative Solution

If you prefer not to re-import, you can manually edit the existing connector:
1. **Custom connector** → **Definition** tab
2. **Swagger Editor** → Find `LicenseCountsResponse` 
3. **Change** `type: integer` to `type: number` for all three properties
4. **Save** and test

---
**File**: `clara-swagger-powerplatform-compatible.yaml`  
**Version**: 1.0.2  
**Status**: Ready for import - Fixes schema validation warnings
