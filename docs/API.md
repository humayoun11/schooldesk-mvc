# SchoolDesk API

Base URL for local development: `http://localhost:5080`. Remote deployments require HTTPS. Cookies alone do not authorise the protected API actions.

## Token

`POST /api/token` with form fields `Email` and `Password`. For example, use Postman's Body → x-www-form-urlencoded and your generated demo credentials. This endpoint does not use an existing browser session. Failed authentication returns 401 with a generic response. Invalid input returns 400.

Response: `access_token`, `token_type: Bearer`, `expires_in: 900`.

## Students

`GET /api/students?page=1` with `Authorization: Bearer <access_token>`.

Returns up to 20 active students. Admin and Accountant see active students; Teacher sees assigned classes; Parent sees linked children; Student sees their own linked record. No parent contact details are returned.

## Revoke

`POST /Api/Revoke` with the bearer header. Revokes all current tokens and website sessions for that account. Tokens also become invalid after password change, account disable or website logout. Changing the user's class/record assignment is reflected immediately because scope is checked against the database on every request.

Tokens expire after 15 minutes and are not refreshed. Keep tokens out of URLs and do not save them in browser localStorage. There is no permissive cross-origin policy.

JWT implementation uses the maintained Microsoft IdentityModel library. An uncommitted random local key is created during setup. Hosting needs separately configured key protection and rotation.
