# Handshakes Integration Tests Setup Guide

This guide provides step-by-step instructions for setting up your local development workstation to run all Chonkie.Net **handshakes integration tests** with vector databases.

> **Note**: For embeddings provider integration tests, see [INTEGRATION_TESTS_SETUP.md](INTEGRATION_TESTS_SETUP.md)

## Table of Contents

- [Prerequisites](#prerequisites)
- [Quick Start - All Services](#quick-start---all-services)
- [Individual Service Setup](#individual-service-setup)
- [Cloud Services Setup](#cloud-services-setup)
- [Running Integration Tests](#running-integration-tests)
- [Troubleshooting](#troubleshooting)
- [Cleanup](#cleanup)

---

## Prerequisites

### Required Software

1. **.NET 10 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
   ```powershell
   dotnet --version  # Should show 10.0.x
   ```

2. **Podman or Docker** - Container runtime for database services
   - **Podman** (Recommended): [Install Guide](https://podman.io/getting-started/installation)
   - **Docker Desktop**: [Download](https://www.docker.com/products/docker-desktop/)
   
   ```powershell
   # Verify installation
   podman --version  # or: docker --version
   ```

3. **Git** - For cloning and version control
   ```powershell
   git --version
   ```

### Optional Requirements

- **OpenAI API Key** - For OpenAI embedding integration tests
- **Pinecone API Key** - For Pinecone handshake tests
- **Turbopuffer API Key** - For Turbopuffer handshake tests

---

## Quick Start - All Services

### Option 1: Using Docker Compose (Recommended)

Create a `docker-compose.handshakes.yml` file in the repository root:

```yaml
version: '3.8'

services:
  # Qdrant Vector Database
  qdrant:
    image: qdrant/qdrant:latest
    container_name: chonkie-qdrant
    ports:
      - "6333:6333"
      - "6334:6334"
    volumes:
      - qdrant_storage:/qdrant/storage
    environment:
      - QDRANT__SERVICE__GRPC_PORT=6334
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:6333/health"]
      interval: 10s
      timeout: 5s
      retries: 5

  # Chroma Vector Database
  chroma:
    image: chromadb/chroma:latest
    container_name: chonkie-chroma
    ports:
      - "8000:8000"
    volumes:
      - chroma_storage:/chroma/chroma
    environment:
      - IS_PERSISTENT=TRUE
      - ANONYMIZED_TELEMETRY=FALSE
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8000/api/v1/heartbeat"]
      interval: 10s
      timeout: 5s
      retries: 5

  # MongoDB with Vector Search Support
  mongodb:
    image: mongo:7.0
    container_name: chonkie-mongodb
    ports:
      - "27017:27017"
    volumes:
      - mongodb_storage:/data/db
    environment:
      - MONGO_INITDB_ROOT_USERNAME=chonkie
      - MONGO_INITDB_ROOT_PASSWORD=chonkie123
      - MONGO_INITDB_DATABASE=chonkie_test
    healthcheck:
      test: ["CMD", "mongosh", "--eval", "db.adminCommand('ping')"]
      interval: 10s
      timeout: 5s
      retries: 5

  # Milvus Dependencies - etcd
  milvus-etcd:
    image: quay.io/coreos/etcd:v3.5.5
    container_name: chonkie-milvus-etcd
    environment:
      - ETCD_AUTO_COMPACTION_MODE=revision
      - ETCD_AUTO_COMPACTION_RETENTION=1000
      - ETCD_QUOTA_BACKEND_BYTES=4294967296
      - ETCD_SNAPSHOT_COUNT=50000
    volumes:
      - milvus_etcd:/etcd
    command: etcd -advertise-client-urls=http://127.0.0.1:2379 -listen-client-urls http://0.0.0.0:2379 --data-dir /etcd
    healthcheck:
      test: ["CMD", "etcdctl", "endpoint", "health"]
      interval: 10s
      timeout: 5s
      retries: 5

  # Milvus Dependencies - MinIO
  milvus-minio:
    image: minio/minio:RELEASE.2023-03-20T20-16-18Z
    container_name: chonkie-milvus-minio
    environment:
      MINIO_ACCESS_KEY: minioadmin
      MINIO_SECRET_KEY: minioadmin
    volumes:
      - milvus_minio:/minio_data
    command: minio server /minio_data
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:9000/minio/health/live"]
      interval: 30s
      timeout: 20s
      retries: 3

  # Milvus Vector Database
  milvus-standalone:
    image: milvusdb/milvus:v2.3.3
    container_name: chonkie-milvus
    command: ["milvus", "run", "standalone"]
    environment:
      ETCD_ENDPOINTS: milvus-etcd:2379
      MINIO_ADDRESS: milvus-minio:9000
    volumes:
      - milvus_storage:/var/lib/milvus
    ports:
      - "19530:19530"
      - "9091:9091"
    depends_on:
      - milvus-etcd
      - milvus-minio
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:9091/healthz"]
      interval: 10s
      timeout: 5s
      retries: 10

  # Elasticsearch with Vector Search
  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:8.11.0
    container_name: chonkie-elasticsearch
    environment:
      - discovery.type=single-node
      - xpack.security.enabled=false
      - "ES_JAVA_OPTS=-Xms512m -Xmx512m"
    ports:
      - "9200:9200"
      - "9300:9300"
    volumes:
      - elasticsearch_storage:/usr/share/elasticsearch/data
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:9200/_cluster/health"]
      interval: 10s
      timeout: 5s
      retries: 10

  # PostgreSQL with pgvector Extension
  postgres:
    image: pgvector/pgvector:pg16
    container_name: chonkie-postgres
    environment:
      - POSTGRES_USER=chonkie
      - POSTGRES_PASSWORD=chonkie123
      - POSTGRES_DB=chonkie_test
    ports:
      - "5432:5432"
    volumes:
      - postgres_storage:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U chonkie"]
      interval: 10s
      timeout: 5s
      retries: 5

  # Weaviate Vector Database
  weaviate:
    image: semitechnologies/weaviate:1.23.1
    container_name: chonkie-weaviate
    ports:
      - "8080:8080"
    environment:
      - QUERY_DEFAULTS_LIMIT=25
      - AUTHENTICATION_ANONYMOUS_ACCESS_ENABLED=true
      - PERSISTENCE_DATA_PATH=/var/lib/weaviate
      - DEFAULT_VECTORIZER_MODULE=none
      - CLUSTER_HOSTNAME=node1
    volumes:
      - weaviate_storage:/var/lib/weaviate
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/v1/.well-known/ready"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  qdrant_storage:
  chroma_storage:
  mongodb_storage:
  milvus_etcd:
  milvus_minio:
  milvus_storage:
  elasticsearch_storage:
  postgres_storage:
  weaviate_storage:
```

### Start All Services

**Using Docker:**
```powershell
docker-compose -f docker-compose.handshakes.yml up -d
```

**Using Podman:**
```powershell
# Option 1: Using podman-compose (install via pip if needed)
podman-compose -f docker-compose.handshakes.yml up -d

# Option 2: Using podman directly
podman play kube docker-compose.handshakes.yml
```

### Wait for Services to be Ready

```powershell
# Check health status
docker-compose -f docker-compose.handshakes.yml ps
# or
podman-compose -f docker-compose.handshakes.yml ps

# All services should show "healthy" status
```

### Verify Services Are Running

```powershell
# Test each service endpoint
curl http://localhost:6333/health          # Qdrant - should return JSON
curl http://localhost:8000/api/v1/heartbeat # Chroma - should return JSON
curl http://localhost:9200                 # Elasticsearch - should return JSON
curl http://localhost:8080/v1/meta         # Weaviate - should return JSON

# Test ports
Test-NetConnection localhost -Port 27017  # MongoDB
Test-NetConnection localhost -Port 19530  # Milvus
Test-NetConnection localhost -Port 5432   # PostgreSQL
```

---

## Individual Service Setup

### 1. Qdrant Vector Database

**Container Setup:**
```powershell
# Using Docker
docker run -d --name chonkie-qdrant \
  -p 6333:6333 -p 6334:6334 \
  -v qdrant_storage:/qdrant/storage \
  -e QDRANT__SERVICE__GRPC_PORT=6334 \
  qdrant/qdrant:latest

# Using Podman
podman run -d --name chonkie-qdrant \
  -p 6333:6333 -p 6334:6334 \
  -v qdrant_storage:/qdrant/storage \
  -e QDRANT__SERVICE__GRPC_PORT=6334 \
  qdrant/qdrant:latest
```

**Verify:**
```powershell
curl http://localhost:6333/health
# Expected: {"title":"qdrant - vector search engine","version":"1.x.x"}
```

**Integration Tests:** `QdrantHandshakeIntegrationTests.cs`
- Total Tests: 4
- Requires: Qdrant running at `http://localhost:6333`
- Optional: `OPENAI_API_KEY` environment variable for OpenAI embedding tests

---

### 2. Chroma Vector Database

**Container Setup:**
```powershell
# Using Docker
docker run -d --name chonkie-chroma \
  -p 8000:8000 \
  -v chroma_storage:/chroma/chroma \
  -e IS_PERSISTENT=TRUE \
  -e ANONYMIZED_TELEMETRY=FALSE \
  chromadb/chroma:latest

# Using Podman
podman run -d --name chonkie-chroma \
  -p 8000:8000 \
  -v chroma_storage:/chroma/chroma \
  -e IS_PERSISTENT=TRUE \
  -e ANONYMIZED_TELEMETRY=FALSE \
  chromadb/chroma:latest
```

**Verify:**
```powershell
curl http://localhost:8000/api/v1/heartbeat
# Expected: {"nanosecond heartbeat": <timestamp>}
```

**Integration Tests:** `ChromaHandshakeIntegrationTests.cs`
- Total Tests: 3
- Requires: Chroma running at `http://localhost:8000`
- Tests: WriteAsync, SearchAsync with local embeddings

---

### 3. MongoDB with Vector Search

**Container Setup:**
```powershell
# Using Docker
docker run -d --name chonkie-mongodb \
  -p 27017:27017 \
  -e MONGO_INITDB_ROOT_USERNAME=chonkie \
  -e MONGO_INITDB_ROOT_PASSWORD=chonkie123 \
  -e MONGO_INITDB_DATABASE=chonkie_test \
  -v mongodb_storage:/data/db \
  mongo:7.0

# Using Podman
podman run -d --name chonkie-mongodb \
  -p 27017:27017 \
  -e MONGO_INITDB_ROOT_USERNAME=chonkie \
  -e MONGO_INITDB_ROOT_PASSWORD=chonkie123 \
  -e MONGO_INITDB_DATABASE=chonkie_test \
  -v mongodb_storage:/data/db \
  mongo:7.0
```

**Verify:**
```powershell
# If mongosh is installed
mongosh "mongodb://chonkie:chonkie123@localhost:27017/chonkie_test"

# Or test connection
Test-NetConnection localhost -Port 27017
```

**Integration Tests:** `MongoDBHandshakeIntegrationTests.cs`
- Total Tests: 3
- Requires: MongoDB running at `localhost:27017`
- Connection: `mongodb://localhost:27017` or with credentials
- Tests: WriteAsync, SearchAsync, brute-force vector search

---

### 4. Milvus Vector Database

**Container Setup:**

Milvus requires etcd and MinIO. Recommended to use docker-compose configuration above.

**Standalone Quick Start:**
```powershell
# Download standalone script (Linux/macOS)
curl -sfL https://raw.githubusercontent.com/milvus-io/milvus/master/scripts/standalone_embed.sh -o standalone.sh
bash standalone.sh start

# Windows: Use docker-compose configuration
```

**Verify:**
```powershell
# Test connection (no HTTP health endpoint by default)
Test-NetConnection localhost -Port 19530

# Check logs
docker logs chonkie-milvus
# Should see: "Milvus Proxy successfully started"
```

**Integration Tests:** `MilvusHandshakeIntegrationTests.cs`
- Total Tests: 3
- Requires: Milvus running at `http://localhost:19530`
- Tests: WriteAsync, SearchAsync with REST API
- Note: Milvus startup can take 60-90 seconds

---

### 5. Elasticsearch with Vector Search

**Container Setup:**
```powershell
# Using Docker
docker run -d --name chonkie-elasticsearch \
  -p 9200:9200 -p 9300:9300 \
  -e "discovery.type=single-node" \
  -e "xpack.security.enabled=false" \
  -e "ES_JAVA_OPTS=-Xms512m -Xmx512m" \
  -v elasticsearch_storage:/usr/share/elasticsearch/data \
  docker.elastic.co/elasticsearch/elasticsearch:8.11.0

# Using Podman
podman run -d --name chonkie-elasticsearch \
  -p 9200:9200 -p 9300:9300 \
  -e "discovery.type=single-node" \
  -e "xpack.security.enabled=false" \
  -e "ES_JAVA_OPTS=-Xms512m -Xmx512m" \
  -v elasticsearch_storage:/usr/share/elasticsearch/data \
  docker.elastic.co/elasticsearch/elasticsearch:8.11.0
```

**Verify:**
```powershell
curl http://localhost:9200
# Expected: JSON response with cluster info

curl http://localhost:9200/_cluster/health
# Expected: {"status":"yellow" or "green",...}
```

**Integration Tests:** `ElasticsearchHandshakeIntegrationTests.cs`
- Total Tests: 3
- Requires: Elasticsearch running at `http://localhost:9200`
- Tests: WriteAsync, SearchAsync with KNN, dense_vector support
- Note: Elasticsearch startup can take 30-60 seconds

---

### 6. PostgreSQL with pgvector Extension

**Container Setup:**
```powershell
# Using Docker
docker run -d --name chonkie-postgres \
  -p 5432:5432 \
  -e POSTGRES_USER=chonkie \
  -e POSTGRES_PASSWORD=chonkie123 \
  -e POSTGRES_DB=chonkie_test \
  -v postgres_storage:/var/lib/postgresql/data \
  pgvector/pgvector:pg16

# Using Podman
podman run -d --name chonkie-postgres \
  -p 5432:5432 \
  -e POSTGRES_USER=chonkie \
  -e POSTGRES_PASSWORD=chonkie123 \
  -e POSTGRES_DB=chonkie_test \
  -v postgres_storage:/var/lib/postgresql/data \
  pgvector/pgvector:pg16
```

**Verify:**
```powershell
# If psql is installed
psql -h localhost -U chonkie -d chonkie_test -c "SELECT version();"
psql -h localhost -U chonkie -d chonkie_test -c "CREATE EXTENSION IF NOT EXISTS vector;"

# Or test connection
Test-NetConnection localhost -Port 5432
```

**Integration Tests:** `PgvectorHandshakeIntegrationTests.cs`
- Total Tests: 3
- Requires: PostgreSQL with pgvector extension
- Connection: `Host=localhost;Database=chonkie_test;Username=chonkie;Password=chonkie123`
- Tests: WriteAsync, SearchAsync, vector similarity

---

### 7. Weaviate Vector Database

**Container Setup:**
```powershell
# Using Docker
docker run -d --name chonkie-weaviate \
  -p 8080:8080 \
  -e QUERY_DEFAULTS_LIMIT=25 \
  -e AUTHENTICATION_ANONYMOUS_ACCESS_ENABLED=true \
  -e PERSISTENCE_DATA_PATH=/var/lib/weaviate \
  -e DEFAULT_VECTORIZER_MODULE=none \
  -e CLUSTER_HOSTNAME=node1 \
  -v weaviate_storage:/var/lib/weaviate \
  semitechnologies/weaviate:1.23.1

# Using Podman
podman run -d --name chonkie-weaviate \
  -p 8080:8080 \
  -e QUERY_DEFAULTS_LIMIT=25 \
  -e AUTHENTICATION_ANONYMOUS_ACCESS_ENABLED=true \
  -e PERSISTENCE_DATA_PATH=/var/lib/weaviate \
  -e DEFAULT_VECTORIZER_MODULE=none \
  -e CLUSTER_HOSTNAME=node1 \
  -v weaviate_storage:/var/lib/weaviate \
  semitechnologies/weaviate:1.23.1
```

**Verify:**
```powershell
curl http://localhost:8080/v1/meta
# Expected: JSON with Weaviate version info

curl http://localhost:8080/v1/.well-known/ready
# Expected: {"status":"running"}
```

**Integration Tests:** `WeaviateHandshakeIntegrationTests.cs`
- Total Tests: 3
- Requires: Weaviate running at `http://localhost:8080`
- Tests: CreateCloudAsync, WriteAsync, SearchAsync

---

## Cloud Services Setup

### Pinecone (Cloud-Only)

**Setup Steps:**
1. Sign up at [https://www.pinecone.io/](https://www.pinecone.io/)
2. Create an API key from the dashboard
3. Create an index:
   - Name: `chonkie-test` (or your preference)
   - Dimension: Must match your embedding model (e.g., 384 for all-MiniLM-L12-v2)
   - Metric: Cosine similarity

**Environment Variables:**
```powershell
# PowerShell (Windows)
$env:PINECONE_API_KEY = "your-api-key-here"
$env:PINECONE_ENVIRONMENT = "gcp-starter"  # or your environment
$env:PINECONE_INDEX_NAME = "chonkie-test"

# Bash (Linux/macOS)
export PINECONE_API_KEY="your-api-key-here"
export PINECONE_ENVIRONMENT="gcp-starter"
export PINECONE_INDEX_NAME="chonkie-test"
```

**Integration Tests:** `PineconeHandshakeIntegrationTests.cs`
- Total Tests: 3
- Requires: `PINECONE_API_KEY`, `PINECONE_ENVIRONMENT`, `PINECONE_INDEX_NAME`
- Tests: Constructor validation, WriteAsync, SearchAsync
- Note: Uses free tier starter environment

---

### Turbopuffer (Cloud-Only)

**Setup Steps:**
1. Sign up at [https://turbopuffer.com/](https://turbopuffer.com/)
2. Get your API key from the dashboard
3. No index creation needed - tables auto-created

**Environment Variables:**
```powershell
# PowerShell (Windows)
$env:TURBOPUFFER_API_KEY = "your-api-key-here"

# Bash (Linux/macOS)
export TURBOPUFFER_API_KEY="your-api-key-here"
```

**Integration Tests:** `TurbopufferHandshakeIntegrationTests.cs`
- Total Tests: 3
- Requires: `TURBOPUFFER_API_KEY`
- Tests: Constructor validation, WriteAsync, SearchAsync
- Note: Uses namespace-based organization

---

### OpenAI Embeddings (Optional)

Some integration tests use OpenAI embeddings for realistic scenarios.

**Setup:**
1. Get API key from [https://platform.openai.com/api-keys](https://platform.openai.com/api-keys)

**Environment Variable:**
```powershell
# PowerShell (Windows)
$env:OPENAI_API_KEY = "sk-..."

# Bash (Linux/macOS)
export OPENAI_API_KEY="sk-..."
```

**Used By:**
- `QdrantHandshakeIntegrationTests.cs` - Tests with OpenAI embeddings
- Various other handshake tests for realistic vector dimensions

---

## Running Integration Tests

### Prerequisites Check

Before running tests, ensure:
1. Required services are running (use `docker-compose ps` or `podman-compose ps`)
2. Services are healthy (green health checks)
3. Model files exist for local embeddings (optional):
   ```
   models/all-MiniLM-L12-v2/
   ```

### Run All Handshake Integration Tests

```powershell
cd C:\Projects\Personal\Chonkie.Net

# Run all handshake integration tests
dotnet test tests/Chonkie.Handshakes.Tests/ `
  --filter "FullyQualifiedName~Integration" `
  --logger "console;verbosity=detailed"
```

### Run Specific Handshake Tests

```powershell
# Qdrant only
dotnet test tests/Chonkie.Handshakes.Tests/ `
  --filter "FullyQualifiedName~QdrantHandshakeIntegration"

# Chroma only
dotnet test tests/Chonkie.Handshakes.Tests/ `
  --filter "FullyQualifiedName~ChromaHandshakeIntegration"

# MongoDB only
dotnet test tests/Chonkie.Handshakes.Tests/ `
  --filter "FullyQualifiedName~MongoDBHandshakeIntegration"

# Milvus only
dotnet test tests/Chonkie.Handshakes.Tests/ `
  --filter "FullyQualifiedName~MilvusHandshakeIntegration"

# Elasticsearch only
dotnet test tests/Chonkie.Handshakes.Tests/ `
  --filter "FullyQualifiedName~ElasticsearchHandshakeIntegration"

# Pgvector only
dotnet test tests/Chonkie.Handshakes.Tests/ `
  --filter "FullyQualifiedName~PgvectorHandshakeIntegration"

# Weaviate only
dotnet test tests/Chonkie.Handshakes.Tests/ `
  --filter "FullyQualifiedName~WeaviateHandshakeIntegration"

# Pinecone only (requires API key)
dotnet test tests/Chonkie.Handshakes.Tests/ `
  --filter "FullyQualifiedName~PineconeHandshakeIntegration"

# Turbopuffer only (requires API key)
dotnet test tests/Chonkie.Handshakes.Tests/ `
  --filter "FullyQualifiedName~TurbopufferHandshakeIntegration"
```

### Test Skipping Behavior

Integration tests use Assert.Skip from `Xunit.v3` package. Tests skipped when:
- Required service is not available (connection refused)
- Required environment variable is not set
- Required model files are not found

**Example output when skipped:**
```
[xUnit.net] Qdrant server not available at http://localhost:6333
Test: QdrantHandshakeIntegrationTests.WriteAsync_WithRealQdrantAndOpenAI_WritesSuccessfully
Result: Skipped
Message: Qdrant server not available at http://localhost:6333

Test summary: total: 4, failed: 0, succeeded: 0, skipped: 4
```

**Example output when passed:**
```
Test: QdrantHandshakeIntegrationTests.WriteAsync_WithRealQdrantAndOpenAI_WritesSuccessfully
Result: Passed
Duration: 2.3s

Test summary: total: 4, failed: 0, succeeded: 4, skipped: 0
```

---

## Troubleshooting

### Service Won't Start

**Check if port is already in use:**
```powershell
# Windows
netstat -ano | findstr :<port>
Get-Process -Id <PID>  # Get process using the port
Stop-Process -Id <PID> -Force  # Kill the process

# Linux/macOS
lsof -i :<port>
kill -9 <PID>
```

**Check container logs:**
```powershell
# Docker
docker logs chonkie-<service-name>
docker logs -f chonkie-<service-name>  # Follow logs

# Podman
podman logs chonkie-<service-name>
podman logs -f chonkie-<service-name>  # Follow logs
```

### Connection Refused Errors

**1. Verify service is running:**
```powershell
docker ps  # or: podman ps
# Look for service in output with "Up" status
```

**2. Check service health:**
```powershell
docker inspect chonkie-<service-name> | Select-String -Pattern "Health"
# or
docker-compose -f docker-compose.handshakes.yml ps
```

**3. Wait for service initialization:**
Some services take time to fully start:
- Elasticsearch: 30-60 seconds
- Milvus: 60-90 seconds
- MongoDB: 10-20 seconds

**4. Restart the container:**
```powershell
docker restart chonkie-<service-name>
# or
podman restart chonkie-<service-name>
```

### Out of Memory Errors

**Symptoms:**
- Container exits with code 137
- Logs show "out of memory" or "OOM killed"

**Solution - Reduce memory usage:**

Edit `docker-compose.handshakes.yml`:
```yaml
elasticsearch:
  environment:
    - "ES_JAVA_OPTS=-Xms256m -Xmx256m"  # Reduce from 512m

milvus-minio:
  deploy:
    resources:
      limits:
        memory: 512M
```

**Solution - Increase Docker memory:**

Docker Desktop:
1. Open Docker Desktop
2. Go to Settings → Resources → Advanced
3. Increase Memory to 4GB or more
4. Click "Apply & Restart"

Podman (Linux):
```bash
# Podman uses system memory, no limit by default
# Check available memory
free -h
```

### Permission Issues (Linux/WSL)

**Volume permission errors:**
```bash
sudo chown -R 1000:1000 /var/lib/docker/volumes/chonkie_*
```

**Podman rootless issues:**
```bash
# Ensure subuid/subgid are configured
cat /etc/subuid
cat /etc/subgid

# If missing, add:
sudo usermod --add-subuids 100000-165535 --add-subgids 100000-165535 $(whoami)
```

### Milvus-Specific Issues

**Milvus won't start or crashes:**

1. **Check dependencies are running:**
   ```powershell
   docker ps | Select-String -Pattern "milvus"
   # Should see: milvus-etcd, milvus-minio, milvus-standalone
   ```

2. **Verify MinIO is accessible:**
   ```powershell
   curl http://localhost:9000/minio/health/live
   ```

3. **Check etcd health:**
   ```powershell
   docker exec chonkie-milvus-etcd etcdctl endpoint health
   ```

4. **Increase startup timeout:**
   Integration tests wait 90 seconds for Milvus. If still failing, manually verify:
   ```powershell
   # Wait 2-3 minutes after starting
   docker logs chonkie-milvus
   # Look for: "Milvus Proxy successfully started"
   ```

### Elasticsearch Yellow Status

**Not an error!** Yellow status is normal for single-node clusters:
```powershell
curl http://localhost:9200/_cluster/health
# "status":"yellow" is OK for testing
# "status":"green" means all shards are allocated
```

### MongoDB Connection String Issues

**If tests fail with authentication errors:**

Update integration tests or use connection string without auth:
```csharp
// Without authentication (if MONGO_INITDB_ROOT_USERNAME not set)
new MongoDBHandshake("localhost", 27017, embeddingModel);

// With authentication
var uri = "mongodb://chonkie:chonkie123@localhost:27017/?authSource=admin";
new MongoDBHandshake(uri, embeddingModel);
```

---

## Cleanup

### Stop All Services

```powershell
# Using Docker Compose
docker-compose -f docker-compose.handshakes.yml stop

# Using Podman Compose
podman-compose -f docker-compose.handshakes.yml stop
```

### Stop and Remove Containers

```powershell
# Using Docker Compose
docker-compose -f docker-compose.handshakes.yml down

# Using Podman Compose
podman-compose -f docker-compose.handshakes.yml down

# Remove individual containers
docker rm -f chonkie-qdrant chonkie-chroma chonkie-mongodb # ... etc
podman rm -f chonkie-qdrant chonkie-chroma chonkie-mongodb # ... etc
```

### Remove All Data (Fresh Start)

**⚠️ WARNING: This deletes all data in the databases!**

```powershell
# Using Docker Compose (removes volumes)
docker-compose -f docker-compose.handshakes.yml down -v

# Manual volume cleanup
docker volume rm qdrant_storage chroma_storage mongodb_storage milvus_storage elasticsearch_storage postgres_storage weaviate_storage
docker volume prune -f

# Podman
podman volume rm qdrant_storage chroma_storage mongodb_storage milvus_storage elasticsearch_storage postgres_storage weaviate_storage
podman volume prune -f
```

### Remove All Images (Complete Cleanup)

```powershell
# Docker
docker rmi qdrant/qdrant chromadb/chroma mongo milvusdb/milvus docker.elastic.co/elasticsearch/elasticsearch pgvector/pgvector semitechnologies/weaviate

# Podman
podman rmi qdrant/qdrant chromadb/chroma mongo milvusdb/milvus docker.elastic.co/elasticsearch/elasticsearch pgvector/pgvector semitechnologies/weaviate
```

---

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Handshakes Integration Tests

on: [push, pull_request]

jobs:
  integration-tests:
    runs-on: ubuntu-latest
    
    services:
      qdrant:
        image: qdrant/qdrant:latest
        ports:
          - 6333:6333
        options: >-
          --health-cmd "curl -f http://localhost:6333/health"
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
      
      chroma:
        image: chromadb/chroma:latest
        ports:
          - 8000:8000
        env:
          IS_PERSISTENT: TRUE
          ANONYMIZED_TELEMETRY: FALSE
      
      mongodb:
        image: mongo:7.0
        ports:
          - 27017:27017
        env:
          MONGO_INITDB_ROOT_USERNAME: chonkie
          MONGO_INITDB_ROOT_PASSWORD: chonkie123
      
      elasticsearch:
        image: docker.elastic.co/elasticsearch/elasticsearch:8.11.0
        ports:
          - 9200:9200
        env:
          discovery.type: single-node
          xpack.security.enabled: false
          ES_JAVA_OPTS: "-Xms512m -Xmx512m"
      
      postgres:
        image: pgvector/pgvector:pg16
        ports:
          - 5432:5432
        env:
          POSTGRES_USER: chonkie
          POSTGRES_PASSWORD: chonkie123
          POSTGRES_DB: chonkie_test
      
      weaviate:
        image: semitechnologies/weaviate:1.23.1
        ports:
          - 8080:8080
        env:
          AUTHENTICATION_ANONYMOUS_ACCESS_ENABLED: true
          PERSISTENCE_DATA_PATH: /var/lib/weaviate
          DEFAULT_VECTORIZER_MODULE: none
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      
      - name: Wait for services
        run: |
          sleep 30
          curl --retry 10 --retry-delay 5 --retry-connrefused http://localhost:6333/health
          curl --retry 10 --retry-delay 5 --retry-connrefused http://localhost:8000/api/v1/heartbeat
          curl --retry 10 --retry-delay 5 --retry-connrefused http://localhost:9200
          curl --retry 10 --retry-delay 5 --retry-connrefused http://localhost:8080/v1/meta
      
      - name: Run Handshake Integration Tests
        run: |
          dotnet test tests/Chonkie.Handshakes.Tests/ \
            --filter "FullyQualifiedName~Integration" \
            --logger "console;verbosity=detailed"
        env:
          OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
          PINECONE_API_KEY: ${{ secrets.PINECONE_API_KEY }}
          TURBOPUFFER_API_KEY: ${{ secrets.TURBOPUFFER_API_KEY }}
```

**Note:** Milvus requires additional setup in CI and may be better tested locally due to complexity.

---

## Summary Checklist

Setup checklist for running all integration tests:

- [ ] Install .NET 10 SDK
- [ ] Install Podman or Docker
- [ ] Create `docker-compose.handshakes.yml` from this guide
- [ ] Start all services: `docker-compose -f docker-compose.handshakes.yml up -d`
- [ ] Wait for services to be healthy (30-90 seconds)
- [ ] Verify each service:
  - [ ] Qdrant: `curl http://localhost:6333/health`
  - [ ] Chroma: `curl http://localhost:8000/api/v1/heartbeat`
  - [ ] MongoDB: `Test-NetConnection localhost -Port 27017`
  - [ ] Milvus: `Test-NetConnection localhost -Port 19530`
  - [ ] Elasticsearch: `curl http://localhost:9200`
  - [ ] PostgreSQL: `Test-NetConnection localhost -Port 5432`
  - [ ] Weaviate: `curl http://localhost:8080/v1/meta`
- [ ] Set environment variables for cloud services (optional):
  - [ ] `OPENAI_API_KEY` (optional, for OpenAI embeddings)
  - [ ] `PINECONE_API_KEY` (for Pinecone tests)
  - [ ] `TURBOPUFFER_API_KEY` (for Turbopuffer tests)
- [ ] Download embedding models (optional, for local tests):
  - [ ] `models/all-MiniLM-L12-v2/` directory exists
- [ ] Run all tests: `dotnet test tests/Chonkie.Handshakes.Tests/ --filter "FullyQualifiedName~Integration"`
- [ ] Check results - skipped tests are OK if services/keys aren't configured

**✅ All services running + environment variables set = Full integration test coverage!**

---

## Test Coverage Summary

| Handshake | Tests | Service Required | Port | Cloud/Local |
|-----------|-------|------------------|------|-------------|
| Qdrant | 4 | Qdrant | 6333 | Local |
| Chroma | 3 | Chroma | 8000 | Local |
| MongoDB | 3 | MongoDB | 27017 | Local |
| Milvus | 3 | Milvus | 19530 | Local |
| Elasticsearch | 3 | Elasticsearch | 9200 | Local |
| Pgvector | 3 | PostgreSQL+pgvector | 5432 | Local |
| Weaviate | 3 | Weaviate | 8080 | Local/Cloud |
| Pinecone | 3 | Pinecone API | - | Cloud |
| Turbopuffer | 3 | Turbopuffer API | - | Cloud |
| **Total** | **28** | 7 local + 2 cloud | - | - |

---

## Related Documentation

- **Embeddings Integration Tests**: [INTEGRATION_TESTS_SETUP.md](INTEGRATION_TESTS_SETUP.md)
- **Development Roadmap**: [DEVELOPMENT_ROADMAP_FEB_2026.md](../DEVELOPMENT_ROADMAP_FEB_2026.md)
- **Status Dashboard**: [STATUS_DASHBOARD.md](../STATUS_DASHBOARD.md)
- **Quick Reference**: [QUICK_REFERENCE_FEB_2026.md](../QUICK_REFERENCE_FEB_2026.md)

---

For questions, issues, or contributions, please file an issue on GitHub or refer to the main project documentation.

**Happy Testing! 🚀**
