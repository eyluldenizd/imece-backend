# Legacy tables (intentionally NOT created)

Schema sync for Imece does **not** auto-create the following legacy / unused tables:

| Table | Reason |
| --- | --- |
| `user_roles` | Replaced by `users.role_id` + `user_company_roles` |
| `content_targets` | Replaced by `content_company_targets` |
| `event_participants` | Not part of the current portal schema surface |
| `photo_albums` | Replaced by `media_folders` / `media_files` |
| `photos` | Replaced by `media_files` |
| `video_albums` | Replaced by `media_folders` / `media_files` |
| `videos` | Replaced by `media_files` |

Also intentionally omitted from `announcements`:

- `author_user_id` — removed from the current entity/query model

If an existing database still contains these objects, schema sync will leave them alone (SafeApply never drops unknown tables).
