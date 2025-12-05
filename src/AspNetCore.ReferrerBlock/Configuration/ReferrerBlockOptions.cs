namespace ReferrerBlock.Configuration;

public class ReferrerBlockOptions
{
    // List of blocked domain extensions
    public HashSet<string> BlockedTLDs { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ".biz.id",
        ".co.id",
        ".icu",
        ".in",
        ".xyz",
    };
    // List of suspicious domain patterns (incomplete or generic domains)
    public HashSet<string> BlockedPatterns { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "cekitchenware",
        "crmlogichub",
        "ctysss",
        "followpathcrm",
        "hiplay704",
        "ikancupang",
        "jbuif",
        "karyapemuda",
        "kawruhbasa",
        "kaysdogs",
        "missywilkinson",
        "parakerja",
        "pvdwatersports",
        "pulbarholdings",
        "raymandservice",
        "rororush",
        "sscgdpracticeset",
        "sharpdrivers",
        "teknovidia",
        "tempatbelajar",
        "toworld123",
    };

    // List of blocked complete domains
    public HashSet<string> BlockedDomains { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "airplanesupdate.com",
        "aksarabrita.com",
        "barondamaluku.com",
        "biayakuliah.id",
        "brandipphoto.com",
        "chfaloans.com",
        "crminsightspro.com",
        "cozynestplans.com",
        "def-atk.com",
        "gazdp.com",
        "genshindb.org",
        "globalinsuranceresourcecenter.com",
        "hellyeahomeland.com",
        "indonesiamapan.com",
        "jy47.top",
        "kebumenupdate.com",
        "kiprahkita.com",
        "liveasiannews.com",
        "lovesyh.com",
        "mitsubishiprice.com",
        "nativeindonesia.com",
        "pengkicau.com",
        "periodcostume.top",
        "mojok.co",
        "pauljimandjoespodcast.com",
        "patioinstallationcompanies.com",
        "porosjambimedia.com",
        "roomvivo.com",
        "scfxdl.com",
        "secondechance.org",
        "selectaproperty.us",
        "seohost.us",
        "soldatiki.info",
        "syrfy.com",
        "teknowarta.com",
        "thebirdlore.com",
        "tipgalore.com",
        "tugassains.com",
        "vivarecharge.com",
        "vtpcys.com",
        "wavpn.click",
        "wartaupdate.com",
        "wagnerdom.local",
        "wellsfederal.com",
        "xlyanghub.com",
        "zhengzhouchendi.com",
    };
}
