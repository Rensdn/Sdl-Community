﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sdl.Community.ExcelTerminology.Model;
using Sdl.Community.ExcelTerminology.Services;
using Sdl.Community.ExcelTerminology.Services.Interfaces;
using Sdl.Terminology.TerminologyProvider.Core;

namespace Sdl.Community.ExcelTerminology
{
    public class TerminologyProviderExcel: AbstractTerminologyProvider
    {
        public const string ExcelUriTemplate = "exceltbx://";


        private readonly List<ExcelEntry> _termEntries;

        private readonly ProviderSettings _providerSettings;

        private readonly ITermSearchService _termSearchService;

        public override string Name => $"{PluginResources.ExcelTerminologyProviderName} ({Path.GetFileName(_providerSettings.TermFilePath)})";
        public override string Description => PluginResources.ExcelTerminologyProviderDescription;
        public override Uri Uri => new Uri(ExcelUriTemplate + _providerSettings.TermFilePath);

        public override IDefinition Definition => new Definition(new List<IDescriptiveField>(), GetDefinitionLanguages());

        public TerminologyProviderExcel(ProviderSettings providerSettings, ITermSearchService termSearchService)
        {
            _providerSettings = providerSettings; 
            var parser = new Parser(_providerSettings);
            var transformerService = new EntryTransformerService(parser);
            var excelTermLoader = new ExcelTermLoaderService(_providerSettings);
            var excelTermProviderService = new ExcelTermProviderService(excelTermLoader, transformerService);
            _termSearchService = termSearchService;

            _termEntries = excelTermProviderService.LoadEntries();

        }

        public override IList<ILanguage> GetLanguages()
        {
            return GetDefinitionLanguages().Cast<ILanguage>().ToList();
        }

        public IList<IDefinitionLanguage> GetDefinitionLanguages()
        {
            var result = new List<IDefinitionLanguage>();

            var sourceLanguage = new DefinitionLanguage
            {
                IsBidirectional = true,
                Locale = _providerSettings.SourceLanguage,
                Name = _providerSettings.SourceLanguage.EnglishName,
                TargetOnly = false
            };

            result.Add(sourceLanguage);

            var targetLanguage = new DefinitionLanguage
            {
                IsBidirectional = true,
                Locale = _providerSettings.TargetLanguage,
                Name = _providerSettings.TargetLanguage.EnglishName,
                TargetOnly = false
            };

            result.Add(targetLanguage);
            return result;
        }

        public override IEntry GetEntry(int id)
        {
            return _termEntries.FirstOrDefault(termEntry => termEntry.Id == id);
        }

        public override IEntry GetEntry(int id, IEnumerable<ILanguage> languages)
        {
            return _termEntries.FirstOrDefault(termEntry => termEntry.Id == id);

        }

        public override IList<ISearchResult> Search(string text, ILanguage source, ILanguage destination, int maxResultsCount, SearchMode mode,
            bool targetRequired)
        {

            var result = new List<ISearchResult>();

            if (mode == SearchMode.Normal)
            {
                _termSearchService.Search(text, _termEntries, maxResultsCount);
            }
          
            return result;
        }

    }
}
