// SPDX-License-Identifier: MIT
pragma solidity >=0.8.2 <0.9.0;

import "@openzeppelin/contracts/token/ERC20/ERC20.sol";
import "@openzeppelin/contracts/access/Ownable.sol";

contract DteamPoints is ERC20, Ownable {
    constructor() ERC20("DteamPoints", "DTP") Ownable(msg.sender) {}

    function mint(address to, uint256 amount) external onlyOwner {
        _mint(to, amount);
    }

    function _update(
        address from,
        address to,
        uint256 value
    ) internal override {
        if (from != address(0) && to != address(0)) {
            revert("DteamPoints: transfers are disabled");
        }
        super._update(from, to, value);
    }

    function approve(address, uint256) public pure override returns (bool) {
        revert("DteamPoints: approvals are disabled");
    }
}
