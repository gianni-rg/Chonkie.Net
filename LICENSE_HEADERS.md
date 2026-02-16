# Apache License Header Guidelines

This document provides guidelines for adding Apache License headers to source files in the Chonkie.Net project.

## File Header Template

All C# source files should include the following Apache License header at the top:

```csharp
// Copyright 2025-2026 Gianni Rosa Gallina and Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
```

## For File Contributions

If you are contributing a new file or significantly modifying an existing file, update the copyright header to:

```csharp
// Copyright 2025-2026 Gianni Rosa Gallina and Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
```

## For Other File Types

### XML/XAML Files

```xml
<!--
  Copyright 2025-2026 Gianni Rosa Gallina and Contributors

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

      http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
-->
```

### Python Files (.py)

```python
# Copyright 2025-2026 Gianni Rosa Gallina and Contributors
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
```

### PowerShell Scripts (.ps1)

```powershell
<#
  Copyright 2025-2026 Gianni Rosa Gallina and Contributors

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

      http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
#>
```

## Important Notes

1. **Year Range**: Use "2025-2026" or the current year range. Update as appropriate.
2. **Contributors**: The header includes "and Contributors" to acknowledge all community contributions under the Apache License 2.0.
3. **Derived Works**: If you significantly modify a file that already has a header, you may update the copyright year range but maintain the original attribution.
4. **When to Add**:
   - All new source files should have the header
   - Substantially new contributions should include the header
   - Minor bug fixes or documentation changes may not require header additions
5. **External Dependencies**: Files that are derived from or closely follow external projects should maintain original copyright notices. See NOTICE file for attribution details.

## Automation

Tools like the following can help automate license header management:

- **Apache Rat**: Checks for proper license headers
- **EditorConfig**: Can enforce header consistency

## Questions?

If you're unsure about copyright headers for your contribution, please ask in the pull request or check existing files in the same directory for examples.
