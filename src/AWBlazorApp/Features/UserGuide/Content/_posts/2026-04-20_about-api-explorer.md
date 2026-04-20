---
title: About the API Explorer
summary: The in-browser API tester at /api-explorer for trying every REST endpoint without leaving the app.
tags: [developer, api]
category: entity-guide
author: AWBlazor
---

The API explorer at `/api-explorer` is an interactive tool for trying AWBlazor's REST API without leaving the browser. It lists every `/api/*` endpoint, shows the expected inputs and outputs, lets you send test requests, and displays the responses. The page is Admin-only because it can exercise any endpoint in the system including ones that mutate data. Non-admin users see Swagger at `/swagger` for the read-only schema reference.

## What the page shows

The explorer groups endpoints by feature — Forecasts, Inventory, Maintenance, and so on — matching the same organization as the nav. Each endpoint shows the HTTP method (GET, POST, PUT, DELETE), the URL pattern, a one-line description, the required role, and a Try-it button. Clicking Try-it expands the endpoint into a request-builder with input fields for path parameters, query parameters, and request body (for POST and PUT).

## Sending a request

When you send a request from the explorer, the call uses your current browser session — the cookie that identifies you is automatically included. You do not need to generate an API key just to try an endpoint. For testing API-key behaviour specifically (checking what a key-based caller would see), the explorer has an "Use API key" toggle that sends requests with a selected key instead of the cookie.

## Viewing the response

Responses are shown in three tabs — Body (the response payload, pretty-printed if JSON), Headers (the full response headers including cache, correlation, and auth-related ones), and cURL (a cURL command line that reproduces the request, useful for copying into scripts or bug reports). The cURL output uses your cookie or API key as appropriate so the command is self-contained.

## Endpoint documentation

Each endpoint has a documentation panel with the full request and response schemas, examples, and any caveats — rate limits, role requirements, deprecation notices. The documentation is generated from the Swagger annotations on the endpoint in code, so it always reflects the current deployed version. If a field is marked deprecated in code, it shows up marked deprecated in the explorer.

## Relationship to Swagger

The Swagger UI at `/swagger` is the same underlying schema, presented as the standard Swagger interface. The API explorer is a richer experience built on top — Swagger is the minimal reference; the explorer adds session-aware authentication, cURL generation, endpoint grouping, and a UI that matches the rest of the app. Both are maintained automatically from the endpoint code; there is no separate documentation step.
