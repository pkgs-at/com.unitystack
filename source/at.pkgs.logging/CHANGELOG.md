At.Pkgs.Logging CHANGELOG
========

1.0.1
--------

### 外部API

#### `BasicLoggingConfiguration`

##### change: 設定ロードメソッド名変更

他のConfigurationとの命名・操作直行性を持たせる目的で実施。

変更前:

    void Configure(
        XmlReader reader)
    
    void Configure(
        Stream input)

変更後:

    void Load(
        XmlReader reader)
    
    void Load(
        Stream stream)

### 拡張API

#### `BasicLoggingConfiguration`

##### change: アペンダ生成メソッド名および引数型変更

設定ファイル仕様変更に伴う改修。
また、`System.Collections.Specialized`名前空間の使用を避ける方針。

変更前:

    Appender virtual CreateAdapter(
        string name,
        NameValueCollection parameters)

変更後:

    Appender virtual CreateAdapterPipelineFinal(
        string name,
        IDictionary<string, string> properties)

### 設定ファイル

#### `BasicLoggingConfiguration`

##### change: `<BasicLoggingConfiguration/><Appender/><Pipeline/>`

変更前:

    <Instance key1="ValueA" key2="ValueB">ConsoleAppender</Instance>

変更後:

    <Final name="ConsoleAppender">
      <Property name="PropA">ValueA</Property>
      <Property name="PropB">ValueB</Property>
    </Final>

##### change: `<BasicLoggingConfiguration/><Log/>`

変更前:

    <Pattern matches="*" level="debug"/>

変更後:

    <Pattern level="debug">*</Pattern>

### 内部実装

- `XmlReader`を直接使用していた記述を、XMLステートマシンと切り離し(`At.Pkgs.Util.XmlScanner`を使用して再実装)
- At.Pkgs.Util由来のコードを移動
