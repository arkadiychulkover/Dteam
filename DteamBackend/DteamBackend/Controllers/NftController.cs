using DteamBackend.Interfaces;
using DteamBackend.Models;
using DteamBackend.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DteamBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class NftController : ControllerBase
    {
        private readonly INftService _nftService;
        private readonly IConfiguration _configuration;

        public NftController(INftService nftService, IConfiguration configuration)
        {
            _nftService = nftService;
            _configuration = configuration;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<NftItem>>> GetAll([FromQuery] NftRarity? rarity, [FromQuery] bool? isMinted)
        {
            var nfts = await _nftService.GetAllNftsAsync(rarity, isMinted);
            return Ok(nfts);
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<ActionResult<NftItem>> GetById(Guid id)
        {
            var nft = await _nftService.GetNftByIdAsync(id);
            if (nft == null)
            {
                return NotFound(new { message = "NFT не знайдено." });
            }
            return Ok(nft);
        }

        [HttpGet("token/{tokenId:int}")]
        public async Task<ActionResult<NftItem>> GetByTokenId(int tokenId)
        {
            var nft = await _nftService.GetNftByTokenIdAsync(tokenId);
            if (nft == null)
            {
                return NotFound(new { message = "NFT з таким токен ID не знайдено." });
            }
            return Ok(nft);
        }

        [HttpGet("user/{userId:guid}/gifts")]
        public async Task<ActionResult<List<NftItem>>> GetUserGifts(Guid userId)
        {
            var gifts = await _nftService.GetUserGiftsAsync(userId);
            return Ok(gifts);
        }

        [HttpGet("{id:guid}/transfers")]
        public async Task<ActionResult<List<NftTransfer>>> GetTransfersByNftId(Guid id)
        {
            var transfers = await _nftService.GetTransfersByNftIdAsync(id);
            return Ok(transfers);
        }

        [HttpGet("token/{tokenId:int}/transfers")]
        public async Task<ActionResult<List<NftTransfer>>> GetTransfersByTokenId(int tokenId)
        {
            var transfers = await _nftService.GetTransfersByTokenIdAsync(tokenId);
            return Ok(transfers);
        }

        [HttpGet("config")]
        public async Task<ActionResult> GetConfig()
        {
            var contractAddress = _configuration["Ethereum:NftContractAddress"] 
                                ?? "0x9fE46736679d2D9a65F0992F2272dE9f3c7fa6e0";
            var totalCount = await _nftService.GetTotalCountAsync();

            return Ok(new
            {
                contractAddress,
                totalCount,
                name = "DNFT",
                symbol = "DNFT"
            });
        }
    }
}
