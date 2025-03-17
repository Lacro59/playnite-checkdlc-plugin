using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckDlc.Models.Nintendo
{
    public class SearchResult
    {
        [SerializationPropertyName("responseHeader")]
        public ResponseHeader ResponseHeader { get; set; }

        [SerializationPropertyName("response")]
        public Response Response { get; set; }
    }

    public class Doc
    {
        [SerializationPropertyName("fs_id")]
        public string FsId { get; set; }

        [SerializationPropertyName("change_date")]
        public DateTime ChangeDate { get; set; }

        [SerializationPropertyName("url")]
        public string Url { get; set; }

        [SerializationPropertyName("type")]
        public string Type { get; set; }

        [SerializationPropertyName("dates_released_dts")]
        public List<DateTime> DatesReleasedDts { get; set; }

        [SerializationPropertyName("price_has_discount_b")]
        public bool PriceHasDiscountB { get; set; }

        [SerializationPropertyName("pretty_date_s")]
        public string PrettyDateS { get; set; }

        [SerializationPropertyName("related_nsuids_txt")]
        public List<string> RelatedNsuidsTxt { get; set; }

        [SerializationPropertyName("price_discount_percentage_f")]
        public int PriceDiscountPercentageF { get; set; }

        [SerializationPropertyName("title")]
        public string Title { get; set; }

        [SerializationPropertyName("sorting_title")]
        public string SortingTitle { get; set; }

        [SerializationPropertyName("related_game_id_s")]
        public string RelatedGameIdS { get; set; }

        [SerializationPropertyName("pretty_dlc_type_s")]
        public string PrettyDlcTypeS { get; set; }

        [SerializationPropertyName("deprioritise_b")]
        public bool DeprioritiseB { get; set; }

        [SerializationPropertyName("hits_i")]
        public int HitsI { get; set; }

        [SerializationPropertyName("pg_s")]
        public string PgS { get; set; }

        [SerializationPropertyName("title_master_s")]
        public string TitleMasterS { get; set; }

        [SerializationPropertyName("required_system_txt")]
        public List<string> RequiredSystemTxt { get; set; }

        [SerializationPropertyName("image_url")]
        public string ImageUrl { get; set; }

        [SerializationPropertyName("price_regular_f")]
        public double PriceRegularF { get; set; }

        [SerializationPropertyName("originally_for_t")]
        public string OriginallyForT { get; set; }

        [SerializationPropertyName("title_extras_txt")]
        public List<string> TitleExtrasTxt { get; set; }

        [SerializationPropertyName("dlc_type_s")]
        public string DlcTypeS { get; set; }

        [SerializationPropertyName("price_sorting_f")]
        public double PriceSortingF { get; set; }

        [SerializationPropertyName("price_lowest_f")]
        public double PriceLowestF { get; set; }

        [SerializationPropertyName("publisher")]
        public string Publisher { get; set; }

        [SerializationPropertyName("related_game_title_s")]
        public string RelatedGameTitleS { get; set; }

        [SerializationPropertyName("excerpt")]
        public string Excerpt { get; set; }

        [SerializationPropertyName("nsuid_txt")]
        public List<string> NsuidTxt { get; set; }

        [SerializationPropertyName("date_from")]
        public DateTime DateFrom { get; set; }

        [SerializationPropertyName("downloads_rank_i")]
        public int DownloadsRankI { get; set; }

        [SerializationPropertyName("_version_")]
        public object Version { get; set; }
    }

    public class Params
    {
        [SerializationPropertyName("q")]
        public string Q { get; set; }

        [SerializationPropertyName("start")]
        public string Start { get; set; }

        [SerializationPropertyName("fq")]
        public string Fq { get; set; }

        [SerializationPropertyName("sort")]
        public string Sort { get; set; }

        [SerializationPropertyName("rows")]
        public string Rows { get; set; }

        [SerializationPropertyName("wt")]
        public string Wt { get; set; }
    }

    public class Response
    {
        [SerializationPropertyName("numFound")]
        public int NumFound { get; set; }

        [SerializationPropertyName("start")]
        public int Start { get; set; }

        [SerializationPropertyName("numFoundExact")]
        public bool NumFoundExact { get; set; }

        [SerializationPropertyName("docs")]
        public List<Doc> Docs { get; set; }
    }

    public class ResponseHeader
    {
        [SerializationPropertyName("status")]
        public int Status { get; set; }

        [SerializationPropertyName("QTime")]
        public int QTime { get; set; }

        [SerializationPropertyName("params")]
        public Params Params { get; set; }
    }
}
