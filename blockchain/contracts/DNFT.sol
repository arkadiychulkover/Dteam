// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts/token/ERC721/ERC721.sol";
import "@openzeppelin/contracts/token/ERC721/extensions/ERC721URIStorage.sol";
import "@openzeppelin/contracts/access/Ownable.sol";

contract DNFT is ERC721, ERC721URIStorage, Ownable {
    uint256 private _nextTokenId;

    constructor() ERC721("DNFT", "DNFT") Ownable(msg.sender) {}

    function safeMintWithId(
        address to,
        uint256 tokenId,
        string memory uri
    ) external onlyOwner {
        _safeMint(to, tokenId);
        _setTokenURI(tokenId, uri);
    }

    function safeMint(
        address to,
        string memory uri
    ) external onlyOwner returns (uint256) {
        uint256 tokenId = _nextTokenId++;
        while (_ownerOf(tokenId) != address(0)) {
            tokenId = _nextTokenId++;
        }
        _safeMint(to, tokenId);
        _setTokenURI(tokenId, uri);

        return tokenId;
    }

    function tokenURI(
        uint256 tokenId
    ) public view override(ERC721, ERC721URIStorage) returns (string memory) {
        return super.tokenURI(tokenId);
    }

    function supportsInterface(
        bytes4 interfaceId
    ) public view override(ERC721, ERC721URIStorage) returns (bool) {
        return super.supportsInterface(interfaceId);
    }
}
