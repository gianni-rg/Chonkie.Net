# API Reference - Tokenizers
**Scope:** Tokenizer implementations and factories.

## Python Reference
- [chonkie/docs/oss/chunkers/token-chunker.mdx](chonkie/docs/oss/chunkers/token-chunker.mdx)
- [docs/PYTHON_NET_BEHAVIOR_DIFFERENCES.md](docs/PYTHON_NET_BEHAVIOR_DIFFERENCES.md)

## Chonkie.Tokenizers

### BaseTokenizer
Abstract base tokenizer with common vocabulary helpers.

Members:
- Constructors: `protected BaseTokenizer()`
- Methods (protected): `int AddToVocabulary(string token)`
- Methods: `abstract IReadOnlyList<int> Encode(string text)`, `abstract string Decode(IReadOnlyList<int> tokens)`, `virtual int CountTokens(string text)`, `virtual IReadOnlyList<IReadOnlyList<int>> EncodeBatch(IEnumerable<string> texts)`, `virtual IReadOnlyList<string> DecodeBatch(IEnumerable<IReadOnlyList<int>> tokenSequences)`, `virtual IReadOnlyList<int> CountTokensBatch(IEnumerable<string> texts)`, `IReadOnlyList<string> GetVocabulary()`, `IReadOnlyDictionary<string, int> GetTokenMapping()`

### CharacterTokenizer
Character-level tokenizer.

Members:
- Constructors: `CharacterTokenizer()`
- Methods: `override IReadOnlyList<int> Encode(string text)`, `override string Decode(IReadOnlyList<int> tokens)`, `override int CountTokens(string text)`, `int CountTokens(ReadOnlySpan<char> text)`, `override string ToString()`

### WordTokenizer
Word-level tokenizer that splits on spaces.

Members:
- Constructors: `WordTokenizer()`
- Methods: `override IReadOnlyList<int> Encode(string text)`, `override string Decode(IReadOnlyList<int> tokens)`, `override int CountTokens(string text)`, `int CountTokens(ReadOnlySpan<char> text)`, `override string ToString()`

### AutoTokenizer
Factory for creating tokenizers by identifier.

Members:
- Methods: `static ITokenizer Create(object tokenizerOrIdentifier)`, `static ITokenizer CreateFromIdentifier(string identifier)`, `static CharacterTokenizer CreateCharacter()`, `static WordTokenizer CreateWord()`

## Notes on Python Parity
- Python supports additional tokenizer backends (tiktoken, Hugging Face). The .NET port currently focuses on character and word tokenizers. See [docs/PYTHON_NET_BEHAVIOR_DIFFERENCES.md](./docs/PYTHON_NET_BEHAVIOR_DIFFERENCES.md) for details.
