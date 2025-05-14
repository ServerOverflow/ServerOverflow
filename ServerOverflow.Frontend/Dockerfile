FROM oven/bun:1 AS build
WORKDIR /src
COPY . .
RUN bun install
RUN bun run build

FROM oven/bun:1 AS runner
WORKDIR /app
COPY --from=build /src/.output/ /app/
ENV NITRO_PORT 3000
ENV NITRO_HOST 0.0.0.0
EXPOSE 3000
CMD ["bun", "/app/server/index.mjs"]
