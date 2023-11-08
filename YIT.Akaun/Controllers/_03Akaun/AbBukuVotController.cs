using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YIT.__Domain.Entities._Statics;
using YIT.__Domain.Entities.Models._03Akaun;
using YIT._DataAccess.Data;
using YIT._DataAccess.Repositories.Interfaces;
using YIT.Akaun.Models.ViewModels.Forms;

namespace YIT.Akaun.Controllers._03Akaun
{
    [Authorize]
    public class AbBukuVotController : Microsoft.AspNetCore.Mvc.Controller
    {
        public const string modul = Modules.kodAbBukuVot;
        public const string namamodul = Modules.namaAbBukuVot;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAbBukuVotRepository<AbBukuVot> _abBukuVotRepository;
        private readonly _IUnitOfWork _unitOfWork;

        public AbBukuVotController(ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IAbBukuVotRepository<AbBukuVot> abBukuVotRepository,
            _IUnitOfWork unitOfWork)
        {
            _context = context;
            _userManager = userManager;
            _abBukuVotRepository = abBukuVotRepository;
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index(
            PenyataFormModel form)
        {
            PopulateForm(form.Tahun1, form.kataKunciDari, form.kataKunciHingga);

            var results = _abBukuVotRepository.GetResults(form.Tahun1, form.kataKunciDari, form.kataKunciHingga);

            return View(results);
        }

        public async Task<IActionResult> Details(
            int? AkCartaId,
            string? Tahun1,
            int JKWId,
            int JPTJId,
            int JBahagianId,
            string? TarDari,
            string? TarHingga
            )
        {
            if (AkCartaId == null )
            {
                return NotFound();
            }

            PopulateDetailsForm(AkCartaId, Tahun1, JKWId, JPTJId, JBahagianId, TarDari, TarHingga);

            var results = await _abBukuVotRepository.GetResultsByDateRangeAsync(AkCartaId, Tahun1, JKWId, JPTJId, JBahagianId, TarDari, TarHingga);

            return View(results);
        }

        private void PopulateDetailsForm(int? akCartaId, string? tahun1, int? jKWId, int? jPTJId, int? jBahagianId, string? tarDari, string? tarHingga)
        {
            ViewBag.AkCartaId = akCartaId;
            ViewBag.JKWId = jKWId;
            ViewBag.JPTJId = jPTJId;    
            ViewBag.JBahagianId = jBahagianId; 
            ViewBag.Tahun1 = tahun1;
            ViewBag.TarDari = tarDari;
            ViewBag.TarHingga = tarHingga;

            if (akCartaId != null) ViewBag.AkCarta = _unitOfWork.AkCartaRepo.GetById((int)akCartaId);

            if (jBahagianId != null) ViewBag.JBahagian = _unitOfWork.JBahagianRepo.GetAllDetailsById((int)jBahagianId);
        }

        private void PopulateForm(string? tahun1, string? kataKunciDari, string? kataKunciHingga)
        {
            ViewBag.Tahun1 = tahun1;
            ViewBag.KataKunciDari = kataKunciDari;
            ViewBag.KataKunciHingga = kataKunciHingga;
        }
    }
}
