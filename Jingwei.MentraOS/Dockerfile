FROM oven/bun:latest AS deps

WORKDIR /app

# copy package manifest first to leverage build cache
COPY package.json bun.lockb* ./

# install production dependencies
RUN bun install --production

FROM oven/bun:alpine AS runner
WORKDIR /app

# copy installed deps from the builder stage
COPY --from=deps /app/node_modules ./node_modules

# copy application source
COPY . .

ENV NODE_ENV=production

# optional: adjust if your app listens on a different port
EXPOSE 3000

ENTRYPOINT ["bun"]
CMD ["src/index.ts"]
