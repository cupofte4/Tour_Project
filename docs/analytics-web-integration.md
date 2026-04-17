# Analytics Web Integration

This note is for the external web teammate who needs to display current mobile app active-user counts from the existing backend.

## Endpoints

- `POST /api/analytics/events`
- `POST /api/analytics/heartbeat`
- `GET /api/admin/metrics/active-users`

## Active User Meaning

An "active user" is currently counted as an anonymous mobile app session whose most recent activity is within the configured analytics window.

- Default window: `3` minutes
- Config key: `Analytics:ActiveUserWindowMinutes`
- Count basis: recent session activity, not authenticated users or accounts

## Event Payloads

### `POST /api/analytics/events`

Used for anonymous app events such as `app_open` and `stall_view`.

```json
{
  "sessionId": "7c2d6d6a8c2e44f89b693d5ef6d0a9b7",
  "anonymousClientId": "f3e32a4aa0c24d8cbbe4c7b7d0e90d18",
  "eventType": "app_open",
  "occurredAtUtc": "2026-04-18T12:00:00Z",
  "platform": "ios",
  "appVersion": "1.0.0"
}
```

Allowed `eventType` values currently accepted by the backend:

- `app_open`
- `heartbeat`
- `stall_view`
- `session_stopped`

Success response:

```json
{}
```

HTTP status: `202 Accepted`

### `POST /api/analytics/heartbeat`

Used by the mobile app while it stays active in the foreground.

```json
{
  "sessionId": "7c2d6d6a8c2e44f89b693d5ef6d0a9b7",
  "anonymousClientId": "f3e32a4aa0c24d8cbbe4c7b7d0e90d18",
  "occurredAtUtc": "2026-04-18T12:00:45Z",
  "platform": "ios",
  "appVersion": "1.0.0"
}
```

Success response:

```json
{}
```

HTTP status: `202 Accepted`

## Metric Response

### `GET /api/admin/metrics/active-users`

Example response:

```json
{
  "activeUsers": 12,
  "windowMinutes": 3,
  "lastUpdatedUtc": "2026-04-18T12:00:00Z"
}
```

Field meanings:

- `activeUsers`: current count of recent anonymous mobile sessions
- `windowMinutes`: backend-configured recency window used for the count
- `lastUpdatedUtc`: timestamp when this metric was computed

## Polling Behavior

Recommended frontend polling:

- Poll every `15` to `30` seconds for a dashboard card
- Treat the endpoint as polling-friendly REST, not realtime
- Use the returned `windowMinutes` value in the UI if you want to explain the metric

Suggested browser-side behavior:

1. Load the metric once on page open
2. Refresh on an interval
3. Show the last successful update time
4. If a poll fails, keep the last good value and retry on the next interval

## Example Frontend Flow

```ts
async function fetchActiveUsers(apiBaseUrl: string) {
  const response = await fetch(`${apiBaseUrl}/api/admin/metrics/active-users`, {
    method: "GET",
    headers: {
      "Accept": "application/json"
    }
  });

  if (!response.ok) {
    throw new Error(`Active user request failed: ${response.status}`);
  }

  return response.json() as Promise<{
    activeUsers: number;
    windowMinutes: number;
    lastUpdatedUtc: string;
  }>;
}

let latestMetric: Awaited<ReturnType<typeof fetchActiveUsers>> | null = null;

async function poll(apiBaseUrl: string) {
  try {
    latestMetric = await fetchActiveUsers(apiBaseUrl);
    renderMetric(latestMetric);
  } catch (error) {
    renderWarning(latestMetric);
  }
}

poll("https://your-api-host");
setInterval(() => poll("https://your-api-host"), 15000);
```

## CORS / Origin Configuration

If the web dashboard is served from a different origin than the API, add that origin to backend configuration.

Backend config:

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5173",
      "https://your-dashboard.example"
    ]
  }
}
```

Notes:

- The backend does not allow all origins by default
- If `Cors:AllowedOrigins` is empty, browser calls from another origin will fail
- Server-to-server requests are not affected by browser CORS rules

## Local / ngrok Demo

### Local backend

Run the API:

```bash
dotnet run --project src/backend/VinhKhanhGuide.Api/VinhKhanhGuide.Api.csproj
```

Default local metric URL:

```text
http://localhost:5000/api/admin/metrics/active-users
```

Use the actual local port shown by ASP.NET Core if it differs.

### ngrok demo

1. Start the backend locally
2. Expose the backend with ngrok
3. Point the mobile app API base URL at the ngrok HTTPS URL
4. Point the web dashboard at the same ngrok HTTPS URL
5. If the dashboard runs in the browser from another origin, add that origin to `Cors:AllowedOrigins`

Example metric URL:

```text
https://your-ngrok-host.ngrok-free.dev/api/admin/metrics/active-users
```

## Known Limitations

- This is a lightweight active-session metric, not a full analytics platform
- Counts are anonymous and session-based
- `stall_view` is accepted by the backend, but the active-user metric itself only exposes the aggregate count
- The metric is polling-based; no websocket or push updates are included in this slice
- The dashboard endpoint is not authenticated in this slice
- Rapid app background/foreground changes can make the count move quickly, which is expected for a live metric
