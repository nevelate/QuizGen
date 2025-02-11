# Creating Plugins

## First steps
* Clone this repository.
* Create new **class library** project.
* Add reference to **TestParser** project.

## Configuring project
Open your project's *.csproj file.
In between the ```<PropertyGroup>``` tags, add the following element:
```xml
<EnableDynamicLoading>true</EnableDynamicLoading>
```

Modify ```<ProjectReference/>``` to **TestParser** like this:
```xml
<ItemGroup>
    <ProjectReference Include="path to TestParser.csproj">
        <Private>false</Private>
        <ExcludeAssets>runtime</ExcludeAssets>
    </ProjectReference>
</ItemGroup>
```

Build your project and copy *.Dlls to **QuizGen** Plugins folder.
