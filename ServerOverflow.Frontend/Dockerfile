FROM --platform=linux/amd64 oven/bun:1-alpine AS base
WORKDIR /usr/src/app
RUN apk --no-cache add openssh g++ make python3 git

FROM base AS build
WORKDIR /src
ENV API_BASE http://nginx:80/api/
ENV BROWSER_API_BASE /api/
COPY package.json bun.lock ./
RUN bun install
COPY . .
RUN bun run build

FROM base AS runner
WORKDIR /app
COPY --from=build /src/.output/ /app/
ENV NITRO_PORT 3000
ENV NITRO_HOST 0.0.0.0
EXPOSE 3000
CMD ["bun", "/app/server/index.mjs"]
