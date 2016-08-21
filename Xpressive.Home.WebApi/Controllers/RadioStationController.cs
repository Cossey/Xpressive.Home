using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Xpressive.Home.Contracts.Services;

namespace Xpressive.Home.WebApi.Controllers
{
    [RoutePrefix("api/v1/radio")]
    public class RadioStationController : ApiController
    {
        private readonly ITuneInRadioStationService _radioStationService;
        private readonly IFavoriteRadioStationService _favoriteRadioStationService;

        public RadioStationController(ITuneInRadioStationService radioStationService, IFavoriteRadioStationService favoriteRadioStationService)
        {
            _radioStationService = radioStationService;
            _favoriteRadioStationService = favoriteRadioStationService;
        }

        [HttpGet, Route("category")]
        public async Task<IEnumerable<object>> GetCategoriesAsync([FromUri] string parentId = null)
        {
            var categories = await _radioStationService.GetCategoriesAsync(parentId);
            var dtos = categories.Select(c => new
            {
                c.Id,
                c.Name
            });
            return dtos;
        }

        [HttpGet, Route("search")]
        public async Task<object> SearchAsync([FromUri] string query)
        {
            var result = await _radioStationService.SearchStationsAsync(query);

            return new
            {
                result.Stations,
                ShowMore = result.ShowMoreId
            };
        }

        [HttpGet, Route("station")]
        public async Task<object> GetStationsAsync([FromUri] string categoryId)
        {
            var stations = await _radioStationService.GetStationsAsync(categoryId);

            return new
            {
                stations.Stations,
                ShowMore = stations.ShowMoreId
            };
        }

        [HttpGet, Route("playing")]
        public async Task<object> GetPlaying([FromUri] string stationId)
        {
            return await _radioStationService.GetStationDetailAsync(stationId);
        }

        [HttpGet, Route("starred")]
        public async Task<IEnumerable<FavoriteRadioStation>> GetFavorites()
        {
            var favorites = await _favoriteRadioStationService.GetAsync();
            return favorites;
        }

        [HttpPut, Route("star")]
        public async Task Star([FromBody] RadioStationDto dto)
        {
            var radioStation = new TuneInRadioStation
            {
                Id = dto.Id,
                Name = dto.Name,
                ImageUrl = dto.ImageUrl
            };

            await _favoriteRadioStationService.AddAsync(radioStation);
        }

        [HttpPut, Route("unstar")]
        public async Task<IHttpActionResult> Unstar([FromBody] RadioStationDto dto)
        {
            var favorites = await _favoriteRadioStationService.GetAsync();
            var favorite = favorites.SingleOrDefault(f => f.Id.Equals(dto.Id, StringComparison.Ordinal));

            if (favorite == null)
            {
                return BadRequest();
            }

            await _favoriteRadioStationService.RemoveAsync(favorite);
            return Ok();
        }

        public class RadioStationDto
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string ImageUrl { get; set; }
        }
    }
}
