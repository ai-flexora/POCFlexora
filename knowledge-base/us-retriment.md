# User Story Format — POCFlexora

## Story Structure
Every request must follow this format:
- **Ticket ID:** FEL-XXX
- **Title:** Short description
- **As a:** (who is the user)
- **I want:** (what they want)
- **So that:** (why/value)
- **Acceptance Criteria:** Given/When/Then

## Branch Naming
feature/FEL-{id}-{short-description}
Example: feature/FEL-003-add-product-get-by-id

## PR Title Format
[Felxora] FEL-{id}: {Story Title}

## PR Body Template
### Summary
{What was done}

### Endpoints Added
| Method | Route | Description |
|--------|-------|-------------|
| GET    | /api/sample/{id} | Get item by ID |

### Tests Added
- `GetById_ValidId_ReturnsOk`
- `GetById_InvalidId_ReturnsNotFound`