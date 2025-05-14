FROM --platform=linux/amd64 oven/bun:1-alpine AS base
WORKDIR /usr/src/app
RUN apk --no-cache add openssh g++ make python3 git

FROM base AS build
WORKDIR /src
COPY . .
RUN bun install
RUN bun run build

FROM base AS runner
WORKDIR /app
COPY --from=build /src/.output/ /app/
ENV NITRO_PORT 3000
ENV NITRO_HOST 0.0.0.0
ENV API_BASE http://nginx:80/api/
ENV BROWSER_API_BASE /api/
EXPOSE 3000
CMD ["bun", "/app/server/index.mjs"]
