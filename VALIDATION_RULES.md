# Validation Rules

FinShark validates requests through FluentValidation and JSON converters.

## Authentication

### Register

- `email` is required and must be a valid email
- `userName` is required, 3 to 50 characters, letters/numbers/underscore/hyphen only
- `password` is required, minimum 8 characters, with uppercase, lowercase, number, and symbol
- `firstName` max 50 characters when provided
- `lastName` max 50 characters when provided

### Login

- `email` is required and must be a valid email
- `password` is required

### Update Profile

- at least one of `userName`, `firstName`, or `lastName` must be provided
- `userName` 3 to 50 characters when provided
- `userName` allows letters, numbers, underscores, and hyphens
- `firstName` max 50 characters when provided
- `lastName` max 50 characters when provided

### Change Password

- `userId` is required
- `currentPassword` is required
- `newPassword` is required
- `newPassword` minimum 8 characters
- `newPassword` must contain uppercase, lowercase, digit, and symbol

### Assign Role

- `userId` is required
- `role` is required
- `role` max 100 characters

## Stocks

### Create Stock

- `symbol` is required
- `symbol` max 10 characters
- `symbol` must use uppercase letters, digits, and dots only
- `companyName` is required
- `companyName` max 255 characters
- `currentPrice` must be greater than 0
- `marketCap` must be at least 0
- `marketCap` max 2 decimal places

### Update Stock

- `id` must be greater than 0
- `symbol` follows create rules when provided
- `companyName` max 255 characters when provided
- `currentPrice` greater than 0 when provided
- `currentPrice` max 2 decimal places when provided
- `marketCap` at least 0 when provided
- `marketCap` max 2 decimal places when provided

### Stock Query

- `pageNumber` > 0 when provided
- `pageSize` between 1 and 100 when provided
- `symbol` max 10 characters when provided
- `companyName` max 255 characters when provided
- `sortBy` must be a valid enum
- `sortDirection` must be a valid enum
- `minPrice`, `maxPrice`, `minMarketCap`, `maxMarketCap` must be at least 0 when provided
- `minPrice <= maxPrice`
- `minMarketCap <= maxMarketCap`

## Comments

### Create Comment

- `stockId` > 0
- `title` required, 3 to 200 characters
- `content` required, 10 to 5000 characters
- `rating` must be between 1 and 5

### Update Comment

- `id` > 0
- `title` 3 to 200 characters when provided
- `content` 10 to 5000 characters when provided
- `rating` between 1 and 5 when provided

### Comment Query

- `pageNumber` > 0 when provided
- `pageSize` between 1 and 100 when provided
- `sortBy` must be a valid enum
- `sortDirection` must be a valid enum
- `minRating` and `maxRating` must be between 1 and 5 when provided
- `minRating <= maxRating`
- for stock-specific comment queries, `stockId` > 0

## JSON Conversion Rules

### `SectorType`

- accepts canonical strings such as `"Technology"`
- accepts legacy numeric codes
- invalid values fail model binding

### `Rating`

- accepts integers or numeric strings
- values must resolve to 1 through 5
- invalid values fail model binding
