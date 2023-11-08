using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Repositories.Interfaces;
using YIT.Akaun.Models.ViewModels.Forms;

namespace YIT.Akaun.Controllers._03Akaun
{
    [Authorize]
    public class AkAkaunController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly _IUnitOfWork _unitOfWork;
        private readonly IAkAkaunRepository<AkAkaun> _akaunRepository;

        public AkAkaunController(_IUnitOfWork unitOfWork,
            IAkAkaunRepository<AkAkaun> akaunRepository)
        {
            _unitOfWork = unitOfWork;
            _akaunRepository = akaunRepository;
        }
        public async Task<IActionResult> Index(
            PenyataFormModel form
            )
        {
            PopulateForm(form.JKWId, form.AkCartaId, form.TarDari1, form.TarHingga1);

            return View( await _akaunRepository.GetResults(form.JKWId,form.AkCartaId,form.TarDari1,form.TarHingga1));
        }

        private void PopulateForm(int? jKWId, int? akCartaId,DateTime? dateFrom, DateTime? dateTo)
        {
            ViewBag.JKW = _unitOfWork.JKWRepo.GetAll();
            
            var carta = _unitOfWork.AkCartaRepo.GetAll();

            ViewBag.AkCarta = carta.OrderBy(carta => carta.Kod).ToList();

            ViewBag.DateFrom = dateFrom;
            ViewBag.DateTo = dateTo;
        }
    }
}
