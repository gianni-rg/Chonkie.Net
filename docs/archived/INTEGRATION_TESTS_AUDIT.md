# Integration Tests Audit - Handshakes

**Date:** February 5, 2026  
**Status:** In Progress

## Summary

Missing integration tests for 8 handshakes that have unit tests but no integration test coverage.

## Handshakes Status Matrix

| Handshake | Unit Tests | Integration Tests | Priority | Notes |
|-----------|-----------|------------------|----------|-------|
| QdrantHandshake | ✅ 7 tests | ✅ 4 tests (skipped) | DONE | Full coverage with WriteAsync & SearchAsync |
| WeaviateHandshake | ✅ 8 tests | ❌ MISSING | HIGH | Need WriteAsync & SearchAsync tests |
| PineconeHandshake | ✅ 10+ tests | ❌ MISSING | HIGH | Need WriteAsync & SearchAsync tests |
| PgvectorHandshake | ✅ 6 tests | ❌ MISSING | HIGH | Need WriteAsync & SearchAsync tests |
| ChromaHandshake | ✅ 7 tests | ❌ MISSING | HIGH | Just added SearchAsync, need integration tests |
| MongoDBHandshake | ✅ 8 tests | ❌ MISSING | HIGH | Just added SearchAsync, need integration tests |
| MilvusHandshake | ✅ 7 tests | ❌ MISSING | HIGH | Just added SearchAsync, need integration tests |
| ElasticsearchHandshake | ✅ 7 tests | ❌ MISSING | HIGH | Just added SearchAsync, need integration tests |
| TurbopufferHandshake | ✅ 6 tests | ❌ MISSING | MEDIUM | Need both unit & integration tests |

## Missing Integration Tests by Category

### Core Handshakes (4)
1. **WeaviateHandshake** - WriteAsync, SearchAsync
2. **PineconeHandshake** - WriteAsync, SearchAsync
3. **PgvectorHandshake** - WriteAsync, SearchAsync, PostgreSQL setup
4. **TurbopufferHandshake** - WriteAsync, SearchAsync

### Optional Handshakes (4) - Just Implemented
5. **ChromaHandshake** - WriteAsync, SearchAsync with Chroma server
6. **MongoDBHandshake** - WriteAsync, SearchAsync with MongoDB
7. **MilvusHandshake** - WriteAsync, SearchAsync with Milvus
8. **ElasticsearchHandshake** - WriteAsync, SearchAsync with Elasticsearch

## Integration Test Template Pattern

Based on `QdrantHandshakeIntegrationTests.cs`:

```csharp
[Fact]
public async Task WriteAsync_WithRealDatabase_WritesSuccessfully()
{
    // Skip conditions (API keys, running instances, etc.)
    if(!IsAvailable)
        Assert.Skip("Database/Service not available. Skipping integration test.");

    // Setup with sample chunks
    // WriteAsync operation
    // Assertions on result.Count and properties
}

[Fact]
public async Task SearchAsync_WithRealDatabase_FindsSimilarChunks()
{
    // Skip conditions
    // Setup & insert test chunks
    // SearchAsync with query
    // Assertions on result count, similarity scores
}

[Fact]
public async Task WriteAsync_WithRandomCollectionName_CreatesUniqueCollections()
{
    // Test idempotency and unique collection names
}
```

## Implementation Plan

### Phase 1: Core Handshakes (Week 20)
- [ ] WeaviateHandshakeIntegrationTests.cs
- [ ] PineconeHandshakeIntegrationTests.cs
- [ ] PgvectorHandshakeIntegrationTests.cs

### Phase 2: Optional Handshakes (Week 21)
- [ ] ChromaHandshakeIntegrationTests.cs
- [ ] MongoDBHandshakeIntegrationTests.cs
- [ ] MilvusHandshakeIntegrationTests.cs
- [ ] ElasticsearchHandshakeIntegrationTests.cs
- [ ] TurbopufferHandshakeIntegrationTests.cs

## API Key / Setup Requirements

| Handshake | Requirement | Env Var | Default |
|-----------|-------------|---------|---------|
| Weaviate | Running Weaviate server | N/A | http://localhost:8080 |
| Pinecone | API key + Index | PINECONE_API_KEY | cloud.pinecone.io |
| Pgvector | PostgreSQL + extension | DATABASE_URL | localhost:5432 |
| Chroma | Running Chroma server | N/A | http://localhost:8000 |
| MongoDB | Running MongoDB | None required | localhost:27017 |
| Milvus | Running Milvus server | N/A | http://localhost:19530 |
| Elasticsearch | Running Elasticsearch | None required | http://localhost:9200 |
| Turbopuffer | API key | TURBOPUFFER_API_KEY | cloud.turbopuffer.com |

## Coverage Goals

- **Unit Tests:** 100% parameter validation ✅
- **Integration Tests:** 95% coverage (WriteAsync, SearchAsync, unique naming) 
- **Total Tests:** 40+ integration tests across all handshakes
- **Skip Handling:** All tests gracefully skip if dependencies unavailable

## Next Steps

1. Create integration test files for each missing handshake
2. Update STATUS_DASHBOARD.md with test counts
3. Update DEVELOPMENT_ROADMAP_FEB_2026.md with timeline
4. Commit all integration tests

---

**Total Missing:** 8 integration test files  
**Estimated Tests:** 40-50 test cases  
**Effort:** 2-3 days  
**Target Completion:** End of Week 20 (Feb 17, 2026)
