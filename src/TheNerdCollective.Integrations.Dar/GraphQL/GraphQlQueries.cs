namespace TheNerdCollective.Integrations.Dar.GraphQL;

public static class GraphQlQueries
{
    public static string FindHusnummerByAddress => $$"""
        query FindHusnummer($search: String!, $virkningstid: DafDateTime, $registreringstid: DafDateTime) {
          DAR_Husnummer(
            first: 10
            virkningstid: $virkningstid
            registreringstid: $registreringstid
            where: { adgangsadressebetegnelse: { startsWith: $search } }
          ) {
            nodes {
              {{GraphQlFieldLists.Husnummer}}
            }
            pageInfo {
              hasNextPage
            }
          }
        }
        """;

    public static string GetNavngivenVejById => """
        query GetNavngivenVej($navngivenVejId: String!, $virkningstid: DafDateTime, $registreringstid: DafDateTime) {
          DAR_NavngivenVej(
            first: 1
            virkningstid: $virkningstid
            registreringstid: $registreringstid
            where: { id_lokalId: { eq: $navngivenVejId } }
          ) {
            nodes {
              id_lokalId
              vejnavn
            }
          }
        }
        """;

    public static string GetBygningById => $$"""
        query GetBygning($bygningId: String!, $virkningstid: DafDateTime, $registreringstid: DafDateTime) {
          BBR_Bygning(
            first: 1
            virkningstid: $virkningstid
            registreringstid: $registreringstid
            where: { id_lokalId: { eq: $bygningId } }
          ) {
            nodes {
              {{GraphQlFieldLists.Bygning}}
            }
          }
        }
        """;

    public static string GetBygningByHusnummer => $$"""
        query GetBygningByHusnummer($husnummerId: String!, $virkningstid: DafDateTime, $registreringstid: DafDateTime) {
          BBR_Bygning(
            first: 50
            virkningstid: $virkningstid
            registreringstid: $registreringstid
            where: { husnummer: { eq: $husnummerId } }
          ) {
            nodes {
              {{GraphQlFieldLists.Bygning}}
            }
          }
        }
        """;

    public static string GetEnhederByBygning => $$"""
        query GetEnheder($bygningId: String!, $virkningstid: DafDateTime, $registreringstid: DafDateTime) {
          BBR_Enhed(
            first: 100
            virkningstid: $virkningstid
            registreringstid: $registreringstid
            where: { bygning: { eq: $bygningId } }
          ) {
            nodes {
              {{GraphQlFieldLists.Enhed}}
            }
          }
        }
        """;

    public static string GetEtagerByBygning => $$"""
        query GetEtager($bygningId: String!, $virkningstid: DafDateTime, $registreringstid: DafDateTime) {
          BBR_Etage(
            first: 100
            virkningstid: $virkningstid
            registreringstid: $registreringstid
            where: { bygning: { eq: $bygningId } }
          ) {
            nodes {
              {{GraphQlFieldLists.Etage}}
            }
          }
        }
        """;

    public static string GetOpgangeByBygning => $$"""
        query GetOpgange($bygningId: String!, $virkningstid: DafDateTime, $registreringstid: DafDateTime) {
          BBR_Opgang(
            first: 100
            virkningstid: $virkningstid
            registreringstid: $registreringstid
            where: { bygning: { eq: $bygningId } }
          ) {
            nodes {
              {{GraphQlFieldLists.Opgang}}
            }
          }
        }
        """;

    public static string GetTekniskeAnlaegByBygning => $$"""
        query GetTekniskeAnlaeg($bygningId: String!, $virkningstid: DafDateTime, $registreringstid: DafDateTime) {
          BBR_TekniskAnlaeg(
            first: 100
            virkningstid: $virkningstid
            registreringstid: $registreringstid
            where: { bygning: { eq: $bygningId } }
          ) {
            nodes {
              {{GraphQlFieldLists.TekniskAnlaeg}}
            }
          }
        }
        """;

    public static string GetGrundById => $$"""
        query GetGrund($grundId: String!, $virkningstid: DafDateTime, $registreringstid: DafDateTime) {
          BBR_Grund(
            first: 1
            virkningstid: $virkningstid
            registreringstid: $registreringstid
            where: { id_lokalId: { eq: $grundId } }
          ) {
            nodes {
              {{GraphQlFieldLists.Grund}}
            }
          }
        }
        """;

    public static string GetGrundJordstykkerByGrund => $$"""
        query GetGrundJordstykker($grundId: String!, $virkningstid: DafDateTime, $registreringstid: DafDateTime) {
          BBR_GrundJordstykke(
            first: 100
            virkningstid: $virkningstid
            registreringstid: $registreringstid
            where: { grund: { eq: $grundId } }
          ) {
            nodes {
              {{GraphQlFieldLists.GrundJordstykke}}
            }
          }
        }
        """;

    public static string GetBygningEjendomsrelationer => $$"""
        query GetBygningEjendomsrelationer($bygningId: String!, $virkningstid: DafDateTime, $registreringstid: DafDateTime) {
          BBR_BygningEjendomsrelation(
            first: 100
            virkningstid: $virkningstid
            registreringstid: $registreringstid
            where: { bygning: { eq: $bygningId } }
          ) {
            nodes {
              {{GraphQlFieldLists.BygningEjendomsrelation}}
            }
          }
        }
        """;

    public static string GetEjendomsrelationById => $$"""
        query GetEjendomsrelation($ejendomsrelationId: String!, $virkningstid: DafDateTime, $registreringstid: DafDateTime) {
          BBR_Ejendomsrelation(
            first: 1
            virkningstid: $virkningstid
            registreringstid: $registreringstid
            where: { id_lokalId: { eq: $ejendomsrelationId } }
          ) {
            nodes {
              {{GraphQlFieldLists.Ejendomsrelation}}
            }
          }
        }
        """;

    public static string GetAllKommuner => $$"""
        # DAGI_Kommuneinddeling — alle kommuner (kræver virkningstid/registreringstid)
        # https://confluence.sdfi.dk/pages/viewpage.action?pageId=199984259
        query GetAllKommuner($virkningstid: DafDateTime, $registreringstid: DafDateTime, $after: String) {
          DAGI_Kommuneinddeling(
            first: 100
            after: $after
            virkningstid: $virkningstid
            registreringstid: $registreringstid
          ) {
            nodes {
              {{GraphQlFieldLists.Kommuneinddeling}}
            }
            pageInfo {
              hasNextPage
              endCursor
            }
          }
        }
        """;

    public static string FindKommuneByPoint => $$"""
        # DAGI_Kommuneinddeling — geometri.contains (EPSG:25832), jf. Datafordeler DAGI-vejledning
        query FindKommuneByPoint($wkt: String!, $virkningstid: DafDateTime, $registreringstid: DafDateTime) {
          DAGI_Kommuneinddeling(
            first: 1
            virkningstid: $virkningstid
            registreringstid: $registreringstid
            where: {
              geometri: {
                contains: {
                  crs: 25832
                  wkt: $wkt
                }
              }
            }
          ) {
            nodes {
              {{GraphQlFieldLists.Kommuneinddeling}}
            }
          }
        }
        """;

    public static string GetAllPostnumre => $$"""
        query GetAllPostnumre($virkningstid: DafDateTime, $registreringstid: DafDateTime, $after: String) {
          DAR_Postnummer(
            first: 100
            after: $after
            virkningstid: $virkningstid
            registreringstid: $registreringstid
            where: { status: { eq: "3" } }
          ) {
            nodes {
              {{GraphQlFieldLists.Postnummer}}
            }
            pageInfo {
              hasNextPage
              endCursor
            }
          }
        }
        """;

    public static string GetPostnummerByCode => $$"""
        query GetPostnummerByCode($postnr: String!, $virkningstid: DafDateTime, $registreringstid: DafDateTime) {
          DAR_Postnummer(
            first: 1
            virkningstid: $virkningstid
            registreringstid: $registreringstid
            where: { postnr: { eq: $postnr }, status: { eq: "3" } }
          ) {
            nodes {
              {{GraphQlFieldLists.Postnummer}}
            }
          }
        }
        """;
}
