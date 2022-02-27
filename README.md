<p align="center">
    <img src="assets/anvil.png" margin="auto"/>
</p>

<div align="center">
    <a href="https://twitter.com/intent/follow?screen_name=blockmountainio">
        <img src="https://img.shields.io/twitter/follow/blockmountainio?style=flat-square&logo=twitter"
            alt="Follow on Twitter"></a>
</div>

<div style="text-align:center">

# Introduction

<p>

Anvil is a simplistic cross-platform desktop wallet aimed at making offline signing and multisig usage easier within the Solana ecosystem.

View the [demo](https://youtu.be/Hu2u83XXcOk).

</p>

</div>

## Features

- Crafting transactions for offline signing w/ nonce account usage
  - From and to regular accounts (aimed at offline signing)
  - From and to multisig accounts
- Signing messages
- Reassembling transactions from messages & signatures and submitting to network
- Creating multisig accounts
- Creating nonce accounts

## Planned

- Improving the codebase
- Cashmere Wallet integration
- Ledger support

## Dependencies

- .NET 6
- Solnet v6.0.0

## Build

To build Anvil you will need to have the [.NET Runtime 6.0.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) installed.

If you wish to build and publish the application for use within the same machine you can run the following command:

```
dotnet publish -c Release -o ./publish
```

The output binaries and executable will be inside `./publish/`


If you wish to publish the application to be executed in an airgapped machine you will need to build the application as self-contained,
 the following examples will do that for the different architectures.

#### Windows

```
dotnet publish -c Release -o ./publish --runtime win-x64 --self-contained true
```

#### macOS x64

```
dotnet publish -c Release -o ./publish --runtime osx.12-x64 --self-contained true
```

#### macOS arm64

```
dotnet publish -c Release -o ./publish --runtime osx.12-arm64 --self-contained true
```

#### Linux x64

```
dotnet publish -c Release -o ./publish --runtime linux-x64 --self-contained true
```

If you wish to publish the application for another runtime, check out the list of available [runtime identifiers](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog#using-rids).


## Contribution

We encourage everyone to contribute, submit issues, PRs, discuss. Every kind of help is welcome.

## Maintainers

* **Hugo** - [murlokito](https://github.com/murlokito)
* **Tiago** - [tiago](https://github.com/tiago18c)

See also the list of [contributors](https://github.com/bmresearch/Solnet/contributors) who participated in this project.

## License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/bmresearch/Solnet/blob/master/LICENSE) file for details