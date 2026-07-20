# Schema decisions and legacy tables

## Active model decisions

### `users.role_id` vs `user_roles`

**Decision:** Keep `users.role_id` (single primary app role FK) as the source of truth
for current CRUD (`UserQueries` / `UserRepository`).

Multi-company authorization uses `user_company_roles` (created by schema sync,
seeded in development). The old junction table `user_roles` is **legacy** and is
**not** created by `ISchemaDefinition`.

Migration path for existing DBs that still have `user_roles`:

1. Ensure each user has a primary `role_id`.
2. Optionally map rows into `user_company_roles` with a company id.
3. Stop writing to `user_roles`; drop later after verification.

### `announcements.author_user_id`

**Decision:** Column is **removed** from the active schema and queries
(`CompanyScopedContentSchemaDefinition`, `AnnouncementQueries`). Aligns with
commit `0efd84a` ("Remove AuthorUserId from announcements").

Existing DBs that still have `author_user_id NOT NULL` must either:

- make the column nullable / drop it, or
- provide a default before app writes, because the API no longer supplies it.

### `azure_object_id` vs `external_user_identities`

`users.azure_object_id` remains for compatibility with current user APIs.
`external_user_identities` is created for the multi-provider identity model and
is used by SQL directory resolution when `Directory:Provider=Sql`.

### Companies / media FK order

Schema sync creates `companies` before `media_folders` / `media_files`.
System seed inserts `DEFAULT` company; development seed also inserts fixed ids
`1001` (Company A) and `2002` (Company B) to match
`DevelopmentAuthenticationDefaults`.

## Legacy tables (NOT auto-created)

| Table | Status | Notes |
|-------|--------|-------|
| `user_roles` | Legacy | Replaced by `users.role_id` + `user_company_roles` |
| `content_targets` | Legacy | Replaced by `content_company_targets` (branch/dept targeting not ported) |
| `event_participants` | Legacy | Repository fully commented out; entity/DTO remain |
| `photo_albums` | Legacy | Replaced by `media_folders` |
| `photos` | Legacy | Replaced by `media_files` |
| `video_albums` | Legacy | Replaced by `media_folders` |
| `videos` | Legacy | Replaced by `media_files` |

Do not drop legacy tables automatically in production until data migration is
confirmed.
