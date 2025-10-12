# BeanShare Database Seed Data

This folder contains the database seeding infrastructure that automatically populates the application with sample data for development and testing purposes.

## Seed Data Overview

### Users (8 total)
- **John Smith** (john.smith@beanshare.com) - Password: `Password123!`
- **Sarah Johnson** (sarah.johnson@beanshare.com) - Password: `Password123!`
- **Mike Wilson** (mike.wilson@beanshare.com) - Password: `Password123!`
- **Emma Davis** (emma.davis@beanshare.com) - Password: `Password123!`
- **Alex Brown** (alex.brown@beanshare.com) - Password: `Password123!`
- **Lisa Martinez** (lisa.martinez@beanshare.com) - Password: `Password123!`
- **David Garcia** (david.garcia@beanshare.com) - Password: `Password123!` (Unverified)
- **Test User** (test@beanshare.com) - Password: `test123`

### Spaces (5 total)

#### 1. Engineering Team
- **Invite Code**: ENG2024
- **Admin**: John Smith, Emma Davis
- **Members**: 6 total (John, Sarah, Mike, Emma, Alex, Test User)
- **Characteristics**: High activity, well-stocked, diverse coffee preferences

#### 2. Marketing Office
- **Invite Code**: MKT2024
- **Admin**: Sarah Johnson, Lisa Martinez
- **Members**: 4 total (Sarah, Emma, Lisa, David)
- **Characteristics**: Moderate activity, regular business hours only

#### 3. Remote Workers Hub
- **Invite Code**: REMOTE24
- **Admin**: Mike Wilson, Test User
- **Members**: 5 total (Mike, John, Alex, Lisa, Test User)
- **Characteristics**: Irregular patterns, flexible schedule

#### 4. Startup Garage
- **Invite Code**: STARTUP1
- **Admin**: Alex Brown, Mike Wilson
- **Members**: 3 total (Alex, John, Mike)
- **Characteristics**: Late night sessions, budget-conscious choices

#### 5. Executive Lounge
- **Invite Code**: EXEC2024
- **Admin**: John Smith, Sarah Johnson, Emma Davis
- **Members**: 3 total (executives only)
- **Characteristics**: Premium coffee only, minimal consumption

### Coffee Stock

Each space has realistic coffee stock history with various brands:
- **Premium**: Jamaica Blue Mountain, Hawaiian Kona, Geisha Panama
- **Mid-Range**: Lavazza, Illy, Starbucks, Peet's
- **Budget**: Folgers, Store Brand
- **Single Origin**: Ethiopian Yirgacheffe, Costa Rica Tarrazú, Colombian Supremo

Purchase history spans the last 30 days with realistic patterns for each space.

### Consumption Data

Realistic consumption patterns over the last 30 days:
- **Weekday patterns**: Higher consumption Monday-Friday
- **Weekend patterns**: Reduced or no consumption (except Startup Garage)
- **Individual patterns**:
  - Heavy drinkers (3-4 cups/day): John
  - Moderate drinkers (2-3 cups/day): Sarah, Mike
  - Light drinkers (1-2 cups/day): Emma, Lisa
  - Variable consumption: Alex, Test User

Coffee types consumed include:
- Espresso
- Cappuccino
- Latte
- Americano
- Flat White
- Macchiato

## Architecture

### IDataSeeder Interface
All seeders implement this interface with an `Order` property to ensure proper execution sequence:
1. UserSeeder (Order: 1)
2. SpaceSeeder (Order: 2)
3. CoffeeStockSeeder (Order: 3)
4. ConsumptionSeeder (Order: 4)

### DatabaseSeeder
Main orchestrator that runs all seeders in the correct order and handles logging.

### Automatic Seeding
The database is automatically seeded on application startup when using the in-memory database provider (development mode).

## Usage

### Manual Seeding
```csharp
await app.Services.SeedDatabaseAsync();
```

### Disable Seeding
Remove or comment out the seeding line in `Program.cs`:
```csharp
// await app.Services.SeedDatabaseAsync();
```

### Custom Seed Data
To add custom seed data:
1. Create a new class implementing `IDataSeeder`
2. Set appropriate `Order` value
3. Add it to the seeders list in `DatabaseSeeder`

## Testing Scenarios

The seed data supports various testing scenarios:

1. **Multi-tenant testing**: Multiple spaces with different member configurations
2. **Role-based access**: Users with Admin and Member roles
3. **Authentication testing**: Verified and unverified users
4. **Coffee tracking**: Realistic consumption and stock patterns
5. **Time-based data**: Historical data for trend analysis
6. **Budget analysis**: Spaces with different spending patterns

## Important Notes

- All passwords are set to `Password123!` for easy testing (except test user: `test123`)
- The data uses fixed GUIDs for consistent references
- Consumption data uses a fixed random seed (42) for reproducible patterns
- The in-memory database is reset on each application restart