# XPatchLib 扩展 For Json

本项目旨在创建 XPatchLib 的针对 Json 格式的扩展包。使用本扩展包的内容可以使程序在使用 XPatchLib 时支持产生/合并 Json 格式增量内容。

## 使用

使用 `XPatchLib.Json` 最简单的方法是通过 XPatchLib.Json NuGet 软件包。 只需将 [NuGet](https://www.nuget.org/packages/XPatchLib.Json/) 包添加到您的VS项目中即可。

## 支持版本

运行时库被构建为便携式类库，支持：

* .NET Framework 2.0 SP1 及以上版本

* .NET Standard 1.0 及以上版本

可以在 Visual Studio 2008 及后续版本中使用 `XPatchLib.Json`。

## 编译及引用

使用 Visual Studio 2017 及以上版本打开 src/XPatchLib.Json.sln 解决方案。

因为项目文件使用了 Visual Studio 2017 中提供的新的 `csproj` 格式，所以 *开发者* 只能使用 Visual Studio 2017 及以上版本，*使用者* 可以在 Visual Studio 2008 及后续版本中使用 `XPatchLib.Json`。

本项目使用NuGet引用了如下程序包：

* [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/) [Github](https://github.com/JamesNK/Newtonsoft.Json)
* [XPatchLib](https://www.nuget.org/packages/XPatchLib/) [Github](https://github.com/GuQiangJS/XPatchLib.Net)
 
