using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace TagalogWebTest.Pages
{
    public class IndexModel : PageModel
    {


        private readonly ILogger<IndexModel> _logger;
        
        public Dictionary<string, List<TagalogListItem>> OurList { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string SelectedList { get; set; }

        [BindProperty(SupportsGet = true)]
        public DifficultyLevel DifficultyLevel { get; set; }
        
        [BindProperty]
        public string SessionData { get; set; }

        public List<TagalogListItem> Questions { get; set; }

        [BindProperty]
        public string QuestionAnswer { get; set; }
        public string AnswerResponse { get; set; }

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            Init();
            if (!string.IsNullOrEmpty(SelectedList))
            {
                Questions = OurList[SelectedList];
                Questions = Questions.OrderBy(x=> Guid.NewGuid()).ToList();
                SessionData = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Questions)));
            }
        }

        private void Init()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (System.IO.File.Exists(Path.Combine(path,"TagalogTest.json")))
            {
                OurList =
                    JsonSerializer.Deserialize<Dictionary<string, List<TagalogListItem>>>(System.IO.File.ReadAllText(Path.Combine(path,"TagalogTest.json")));
            }
        }

        public IActionResult OnPostStartTestAsync()
        {
            return RedirectToPage("Index", new {SelectedList, DifficultyLevel});
        }
        
        public IActionResult OnPostAnswerAsync()
        {
            Questions = JsonSerializer.Deserialize<List<TagalogListItem>>(
                Encoding.UTF8.GetString(Convert.FromBase64String(SessionData)));

            if (QuestionAnswer?.Equals((DifficultyLevel == DifficultyLevel.Easy
                    ? Questions.First().English
                    : Questions.First().Tagalog), StringComparison.CurrentCultureIgnoreCase) ?? false)
            {
                AnswerResponse = "Great job, you are rocking this.";
                Questions.RemoveAt(0);
            }
            else
            {
                AnswerResponse = $"Oops, {Questions.First().Tagalog}: {Questions.First().English} - That's going to cost ya big time.";
                Questions.Add(Questions.First());
                Questions = Questions.OrderBy(x => Guid.NewGuid()).ToList();
            }

            ModelState.Clear();
            Init();
            SessionData = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Questions)));
            QuestionAnswer = "";
            return Page();
        }
    }

    public enum DifficultyLevel
    {
        Easy,
        Hard
    }
}
