# WebApiToOpenApiReflector
A dotnet tool that can use reflection to generate OpenApi from .NetFramework assemblies

## Intent of this tool
This tool is to help with the recent removal of the `webapi2openapi` command from NSwag.
This tool does not replace NSwag, but is a pre-processor utility to
generate the swagger specifications using reflection on .NET Framework assemblies.
This tool is not recommended for .NET Core assemblies, as the NSwag CLI already supports it.

## Installation
```bash
dotnet tool install -g WebApiToOpenApiReflector
```

## Usage
<!--- BEGIN_TOOL_DOCS --->
```
Usage: WebApiToOpenApiReflector [--controller-names <String>...] [--default-url-template <String>] [--add-missing-path-parameters] [--default-response-reference-type-null-handling <ReferenceTypeNullHandling>] [--generate-original-parameter-names=<true|false>] [--title <String>] [--description <String>] [--info-version <String>] [--document-template <String>] [--help] [--version] assembly-paths0 ... assembly-pathsN

Generates a Swagger/OpenAPI specification for a controller or controllers contained in a .NET Web API assembly.

Arguments:
  0: assembly-paths    The assembly or assemblies to process. (Required)

Options:
  -c, --controller-names <String>...                                                 Can optionally be used to limit the results to some controllers.
  -u, --default-url-template <String>                                                The Web API default URL template: (default for Web API: 'api/{controller}/{id}'; (default for MVC: '{controller}/{action}/{id?}'). (Default: api/{controller}/{id})
  -a, --add-missing-path-parameters                                                  If true, adds missing path parameters which are missing in the action method.
  -n, --default-response-reference-type-null-handling <ReferenceTypeNullHandling>    Specifies the default null handling for reference types when no nullability information is available. NotNull (default) or Null. (Default: Null) (Allowed values: Null, NotNull)
  -g, --generate-original-parameter-names=<true|false>                               Generate x-originalName properties when parameter name is different in .NET and HTTP. (Default: True)
  -t, --title <String>                                                               Specifies the title of the Swagger specification, ignored when the document template is provided.
  -d, --description <String>                                                         Specifies the description of the Swagger specification, ignored when the document template is provided.
  -v, --info-version <String>                                                        Specifies the version of the Swagger specification (default: 1.0.0). (Default: 1.0.0)
  --document-template <String>                                                       Specifies the Swagger document template (may be a path or JSON, default: none).
  -h, --help                                                                         Show help message
  --version                                                                          Show version
```
<!--- END_TOOL_DOCS --->
