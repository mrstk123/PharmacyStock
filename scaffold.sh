# Run this as ONE LINE in Terminal
#dotnet ef dbcontext scaffold "Server=localhost;Database=PharmacyStockDb;User=sa;Password=1234Qwer;TrustServerCertificate=true" Microsoft.EntityFrameworkCore.SqlServer --project PharmacyStock.Infrastructure/PharmacyStock.Infrastructure.csproj --startup-project PharmacyStock.API/PharmacyStock.API.csproj --context-dir "Persistence/Context" --context-namespace PharmacyStock.Infrastructure.Persistence.Context --output-dir "../PharmacyStock.Domain/Entities" --namespace PharmacyStock.Domain.Entities --context AppDbContext --no-onconfiguring --force

# OR

# Bash
#!/bin/bash

# ============================================
# PharmacyStock Scaffolding Script
# Run this from Terminal
# ============================================

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}=========================================${NC}"
echo -e "${YELLOW}PharmacyStock Database Scaffolding Tool${NC}"
echo -e "${BLUE}=========================================${NC}"
echo ""

# Function to print step messages
step() {
    echo -e "${YELLOW}➤ Step $1: $2${NC}"
}

# Function to print success messages
success() {
    echo -e "${GREEN}✓ $1${NC}"
}

# Function to print error messages
error() {
    echo -e "${RED}✗ $1${NC}"
}

# ============================================
# Step 1: Verify Current Directory
# ============================================
step "1" "Verifying solution structure..."

SOLUTION_DIR=$(pwd)
echo "Current directory: $SOLUTION_DIR"

# Check for solution file
if ls *.sln 1> /dev/null 2>&1; then
    success "Solution file found"
else
    error "No .sln file found in current directory"
    echo "Please run this script from your solution root directory"
    exit 1
fi

# ============================================
# Step 2: Check Required Projects
# ============================================
step "2" "Checking required projects..."

REQUIRED_PROJECTS=("PharmacyStock.API" "PharmacyStock.Infrastructure" "PharmacyStock.Domain")
MISSING_PROJECTS=()

for project in "${REQUIRED_PROJECTS[@]}"; do
    if [ -d "$project" ]; then
        success "$project found"
    else
        error "$project not found"
        MISSING_PROJECTS+=("$project")
    fi
done

if [ ${#MISSING_PROJECTS[@]} -gt 0 ]; then
    error "Missing projects: ${MISSING_PROJECTS[*]}"
    exit 1
fi

# ============================================
# Step 3: Database Connection Configuration
# ============================================
step "3" "Configuring database connection..."

# Connection string - EDIT THIS FOR YOUR ENVIRONMENT
DB_SERVER="localhost"
DB_NAME="PharmacyStockDb"
DB_USER="sa"
DB_PASSWORD="1234Qwer" 

CONNECTION_STRING="Server=$DB_SERVER;Database=$DB_NAME;User=$DB_USER;Password=$DB_PASSWORD;TrustServerCertificate=true"

echo ""
echo -e "${YELLOW}Connection Details:${NC}"
echo "Server: $DB_SERVER"
echo "Database: $DB_NAME"
echo "User: $DB_USER"
echo ""

# ============================================
# Step 4: Clean Solution
# ============================================
step "4" "Cleaning solution..."
dotnet clean
if [ $? -eq 0 ]; then
    success "Clean completed"
else
    error "Clean failed"
fi

# ============================================
# Step 5: Restore Packages
# ============================================
step "5" "Restoring NuGet packages..."
dotnet restore
if [ $? -eq 0 ]; then
    success "Restore completed"
else
    error "Restore failed"
    exit 1
fi

# ============================================
# Step 6: Build Solution
# ============================================
step "6" "Building solution..."
dotnet build --verbosity minimal
if [ $? -eq 0 ]; then
    success "Build completed"
else
    error "Build failed - please fix compilation errors first"
    exit 1
fi

# ============================================
# Step 7: Execute Scaffolding
# ============================================
step "7" "Starting database scaffolding..."
echo ""
echo -e "${YELLOW}Scaffolding command:${NC}"
echo "dotnet ef dbcontext scaffold \"$CONNECTION_STRING\" Microsoft.EntityFrameworkCore.SqlServer \\"
echo "  --project PharmacyStock.Infrastructure/PharmacyStock.Infrastructure.csproj \\"
echo "  --startup-project PharmacyStock.API/PharmacyStock.API.csproj \\"
echo "  --context-dir \"Persistence/Context\" \\"
echo "  --context-namespace PharmacyStock.Infrastructure.Persistence.Context \\"
echo "  --output-dir \"../PharmacyStock.Domain/Entities\" \\"
echo "  --namespace PharmacyStock.Domain.Entities \\"
echo "  --context AppDbContext \\"
echo "  --no-onconfiguring \\"
echo "  --force"
echo ""

echo -e "${YELLOW}This will overwrite existing entities and context. Continue? (y/N)${NC}"
read -r response
if [[ ! "$response" =~ ^([yY][eE][sS]|[yY])$ ]]; then
    echo "Scaffolding cancelled."
    exit 0
fi

echo ""
echo -e "${YELLOW}Scaffolding in progress...${NC}"
echo ""

# Execute the scaffolding command
dotnet ef dbcontext scaffold "$CONNECTION_STRING" Microsoft.EntityFrameworkCore.SqlServer \
  --project PharmacyStock.Infrastructure/PharmacyStock.Infrastructure.csproj \
  --startup-project PharmacyStock.API/PharmacyStock.API.csproj \
  --context-dir "Persistence/Context" \
  --context-namespace PharmacyStock.Infrastructure.Persistence.Context \
  --output-dir "../PharmacyStock.Domain/Entities" \
  --namespace PharmacyStock.Domain.Entities \
  --context AppDbContext \
  --no-onconfiguring \
  --force

# ============================================
# Step 8: Verify Results
# ============================================
if [ $? -eq 0 ]; then
    echo ""
    success "Scaffolding completed successfully!"
    echo ""
    echo -e "${YELLOW}Generated Files:${NC}"
    echo "────────────────────────────────────"
    
    # List generated entity files
    ENTITY_COUNT=$(find PharmacyStock.Domain/Entities -name "*.cs" -type f | wc -l)
    echo -e "${GREEN}Entities ($ENTITY_COUNT files):${NC}"
    find PharmacyStock.Domain/Entities -name "*.cs" -type f | sort | sed 's/^/  /'
    
    # List generated context file
    echo ""
    echo -e "${GREEN}DbContext:${NC}"
    if [ -f "PharmacyStock.Infrastructure/Persistence/Context/AppDbContext.cs" ]; then
        echo "  PharmacyStock.Infrastructure/Persistence/Context/AppDbContext.cs"
        success "✓ Context file created"
    else
        error "✗ Context file not found"
    fi
    
    echo ""
    echo -e "${YELLOW}Project Structure:${NC}"
    echo "PharmacyStock.Domain/"
    echo "  └── Entities/           # Domain entities"
    echo ""
    echo "PharmacyStock.Infrastructure/"
    echo "  └── Persistence/"
    echo "      └── Context/"
    echo "          └── AppDbContext.cs"
    echo ""
    
else
    error "Scaffolding failed!"
    exit 1
fi

# ============================================
# Step 9: Optional - Create Migrations
# ============================================
#echo -e "${YELLOW}Would you like to create an initial migration? (y/N)${NC}"
#read -r migration_response
#if [[ "$migration_response" =~ ^([yY][eE][sS]|[yY])$ ]]; then
#    step "9" "Creating initial migration..."
#    
#    cd PharmacyStock.Infrastructure
#    dotnet ef migrations add InitialCreate \
#        --context AppDbContext \
#        --output-dir "Persistence/Migrations" \
#        --startup-project ../PharmacyStock.API/PharmacyStock.API.csproj
#    
#    if [ $? -eq 0 ]; then
#        success "Migration created: PharmacyStock.Infrastructure/Persistence/Migrations/"
#    else
#        error "Migration creation failed"
#    fi
#    cd ..
#fi

echo ""
echo -e "${GREEN}=========================================${NC}"
echo -e "${GREEN}Scaffolding process completed!${NC}"
echo -e "${GREEN}=========================================${NC}"