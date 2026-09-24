using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using CommunityServiceProject.Models;
using CommunityServiceProject.ViewModels;

namespace CommunityServiceProject.Controllers
{
    public class MunicipalAssetController : Controller
    {
        private readonly Community db = new Community();

        /*
         * Reusable HttpClient.
         * Avoids creating a new HttpClient for every request.
         */
        private static readonly HttpClient HttpClient = CreateHttpClient();

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();

            client.Timeout = TimeSpan.FromSeconds(15);

            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "CommunityServiceProject/1.0 (municipal asset management system)"
            );

            return client;
        }

        /*
         * Prevents rapid repeated requests to the public
         * Nominatim service.
         */
        private static readonly object NominatimLock = new object();
        private static DateTime LastNominatimRequestUtc = DateTime.MinValue;

        private static void RespectNominatimRateLimit()
        {
            lock (NominatimLock)
            {
                var elapsed =
                    DateTime.UtcNow - LastNominatimRequestUtc;

                if (elapsed.TotalMilliseconds < 1100)
                {
                    var delay =
                        TimeSpan.FromMilliseconds(
                            1100 - elapsed.TotalMilliseconds
                        );

                    System.Threading.Thread.Sleep(delay);
                }

                LastNominatimRequestUtc = DateTime.UtcNow;
            }
        }

        // ============================================================
        // ADMINISTRATOR AUTHORIZATION
        // ============================================================

        private bool IsAdministrator()
        {
            return Session["AdministratorID"] != null;
        }

        private int? GetAdministratorID()
        {
            if (Session["AdministratorID"] == null)
                return null;

            int administratorID;

            if (int.TryParse(
                Session["AdministratorID"].ToString(),
                out administratorID))
            {
                return administratorID;
            }

            return null;
        }

        // ============================================================
        // ASSET CLASSIFICATION
        // ============================================================

        private Dictionary<string, List<string>> GetAssetCategories()
        {
            return new Dictionary<string, List<string>>
            {
                {
                    "Road Infrastructure",
                    new List<string>
                    {
                        "Road"
                    }
                },

                {
                    "Street Lighting",
                    new List<string>
                    {
                        "Streetlight",
                        "High-Mast Light"
                    }
                },

                {
                    "Water Infrastructure",
                    new List<string>
                    {
                        "Water Pipe",
                        "Water Valve",
                        "Fire Hydrant"
                    }
                },

                {
                    "Stormwater Infrastructure",
                    new List<string>
                    {
                        "Stormwater Drain",
                        "Drainage Channel"
                    }
                },

                {
                    "Traffic Infrastructure",
                    new List<string>
                    {
                        "Traffic Signal"
                    }
                },

                {
                    "Waste Management",
                    new List<string>
                    {
                        "Waste Container"
                    }
                },

                {
                    "Public Facilities",
                    new List<string>
                    {
                        "Community Facility"
                    }
                },

                {
                    "Parks and Recreation",
                    new List<string>
                    {
                        "Park Facility"
                    }
                },

                {
                    "Municipal Buildings",
                    new List<string>
                    {
                        "Municipal Building"
                    }
                }
            };
        }

        private void PopulateAssetClassificationOptions(
            MunicipalAssetCreateViewModel model)
        {
            var classification = GetAssetCategories();

            model.AssetTypeOptions =
                classification.Keys
                    .Select(x => new SelectListItem
                    {
                        Text = x,
                        Value = x
                    })
                    .ToList();

            var selectedCategories =
                !string.IsNullOrWhiteSpace(model.AssetType) &&
                classification.ContainsKey(model.AssetType)
                    ? classification[model.AssetType]
                    : new List<string>();

            model.AssetCategoryOptions =
                selectedCategories
                    .Select(x => new SelectListItem
                    {
                        Text = x,
                        Value = x
                    })
                    .ToList();
        }


        private void PopulateAssetCreateOptions(
            MunicipalAssetCreateViewModel model)
        {
            PopulateAssetClassificationOptions(model);

            var wards =
                db.Wards
                    .AsNoTracking()
                    .ToList()
                    .OrderBy(w =>
                    {
                        int number;

                        return int.TryParse(
                            w.WardNumber,
                            out number)
                            ? number
                            : int.MaxValue;
                    })
                    .ToList();

            model.WardOptions =
                wards
                    .Select(w => new SelectListItem
                    {
                        Text =
                            "Ward " +
                            w.WardNumber +
                            " – " +
                            w.WardName,

                        Value =
                            w.WardID.ToString(),

                        Selected =
                            model.WardID.HasValue &&
                            w.WardID == model.WardID.Value
                    })
                    .ToList();
        }

        private void PopulateAssetEditOptions(
    MunicipalAssetEditViewModel model)
        {
            PopulateAssetClassificationOptionsForEdit(model);

            model.WardOptions =
                db.Wards
                    .AsNoTracking()
                    .ToList()
                    .OrderBy(w =>
                    {
                        int number;

                        return int.TryParse(
                            w.WardNumber,
                            out number)
                            ? number
                            : int.MaxValue;
                    })
                    .Select(w => new SelectListItem
                    {
                        Text =
                            "Ward " +
                            w.WardNumber +
                            " – " +
                            w.WardName,

                        Value =
                            w.WardID.ToString(),

                        Selected =
                            w.WardID == model.WardID
                    })
                    .ToList();
        }

        private List<Ward> GetOrderedWards()
        {
            return db.Wards
                .AsNoTracking()
                .ToList()
                .OrderBy(w =>
                {
                    int number;

                    return int.TryParse(
                        w.WardNumber,
                        out number)
                        ? number
                        : int.MaxValue;
                })
                .ToList();
        }



        
        private void PopulateAssetClassificationOptionsForEdit(
            MunicipalAssetEditViewModel model)
        {
            var classification = GetAssetCategories();

            model.AssetTypeOptions =
                classification.Keys
                    .Select(x => new SelectListItem
                    {
                        Text = x,
                        Value = x,
                        Selected = x == model.AssetType
                    })
                    .ToList();

            var selectedCategories =
                !string.IsNullOrWhiteSpace(model.AssetType) &&
                classification.ContainsKey(model.AssetType)
                    ? classification[model.AssetType]
                    : new List<string>();

            model.AssetCategoryOptions =
                selectedCategories
                    .Select(x => new SelectListItem
                    {
                        Text = x,
                        Value = x,
                        Selected = x == model.AssetCategory
                    })
                    .ToList();
        }

        // ============================================================
        // ASSET DASHBOARD
        // ============================================================

        [HttpGet]
        public ActionResult Dashboard()
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            var assets = db.MunicipalAssets
                .AsNoTracking()
                .OrderByDescending(a => a.DateRegistered)
                .ToList();

            var model = new MunicipalAssetDashboardViewModel
            {
                // ========================================================
                // SUMMARY COUNTS
                // ========================================================

                TotalAssets =
                    assets.Count,

                ActiveAssets =
                    assets.Count(a =>
                        a.Status == AssetStatus.Active),

                RequiresAttention =
                    assets.Count(a =>
                        a.Status == AssetStatus.RequiresAttention),

                UnderMaintenance =
                    assets.Count(a =>
                        a.Status == AssetStatus.UnderMaintenance),

                InactiveAssets =
                    assets.Count(a =>
                        a.Status == AssetStatus.Inactive),

                RetiredAssets =
                    assets.Count(a =>
                        a.Status == AssetStatus.Retired),

                // ========================================================
                // CONDITION COUNTS
                // ========================================================

                ExcellentAssets =
                    assets.Count(a =>
                        a.Condition == AssetCondition.Excellent),

                GoodAssets =
                    assets.Count(a =>
                        a.Condition == AssetCondition.Good),

                FairAssets =
                    assets.Count(a =>
                        a.Condition == AssetCondition.Fair),

                PoorAssets =
                    assets.Count(a =>
                        a.Condition == AssetCondition.Poor),

                CriticalAssets =
                    assets.Count(a =>
                        a.Condition == AssetCondition.Critical),

                // ========================================================
                // ATTENTION REQUIRED
                // ========================================================

                AttentionAssets =
                    assets
                        .Where(a =>
                            a.Status == AssetStatus.RequiresAttention ||
                            a.Condition == AssetCondition.Poor ||
                            a.Condition == AssetCondition.Critical)
                        .OrderByDescending(a => a.DateRegistered)
                        .Take(5)
                        .ToList(),

                // ========================================================
                // RECENT ASSETS
                // ========================================================

                RecentAssets =
                    assets
                        .Take(5)
                        .ToList(),

                DashboardDate =
                    DateTime.Now
            };

            return View(model);
        }

  
// INDEX / US72
[HttpGet]
public ActionResult Index(
    string searchTerm,
    string selectedType,
    string selectedWard,
    string selectedCondition,
    string selectedStatus)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            var query = db.MunicipalAssets
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();

                query = query.Where(a =>
                    a.AssetCode.Contains(searchTerm) ||
                    a.AssetName.Contains(searchTerm) ||
                    a.Description.Contains(searchTerm) ||
                    a.LocationDescription.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(selectedType))
            {
                query = query.Where(a =>
                    a.AssetType == selectedType);
            }

            int selectedWardID;

            if (!string.IsNullOrWhiteSpace(selectedWard) &&
                int.TryParse(selectedWard, out selectedWardID))
            {
                query = query.Where(a =>
                    a.WardID == selectedWardID);
            }

            AssetCondition condition;

            if (!string.IsNullOrWhiteSpace(selectedCondition) &&
                Enum.TryParse(
                    selectedCondition,
                    true,
                    out condition))
            {
                query = query.Where(a =>
                    a.Condition == condition);
            }

            AssetStatus status;

            if (!string.IsNullOrWhiteSpace(selectedStatus) &&
                Enum.TryParse(
                    selectedStatus,
                    true,
                    out status))
            {
                query = query.Where(a =>
                    a.Status == status);
            }

            var assets = query
                .Include(a => a.Ward)
                .OrderBy(a => a.AssetCode)
                .ToList();

            var allAssets =
                db.MunicipalAssets
                    .AsNoTracking();

            var model =
                new MunicipalAssetIndexViewModel
                {
                    Assets = assets,

                    SearchTerm = searchTerm,

                    SelectedType = selectedType,

                    SelectedWard = selectedWard,

                    SelectedCondition =
                        selectedCondition,

                    SelectedStatus =
                        selectedStatus,

                    TotalAssets =
                        allAssets.Count(),

                    ActiveAssets =
                        allAssets.Count(
                            a => a.Status ==
                                 AssetStatus.Active),

                    AttentionAssets =
                        allAssets.Count(
                            a => a.Status ==
                                 AssetStatus.RequiresAttention),

                    UnderMaintenanceAssets =
                        allAssets.Count(
                            a => a.Status ==
                                 AssetStatus.UnderMaintenance),

                    RetiredAssets =
                        allAssets.Count(
                            a => a.Status ==
                                 AssetStatus.Retired)
                };

            ViewBag.AssetTypes =
                db.MunicipalAssets
                    .AsNoTracking()
                    .Where(a => a.AssetType != null)
                    .Select(a => a.AssetType)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToList();

            ViewBag.AssetWards =
    db.Wards
        .AsNoTracking()
        .ToList()
        .OrderBy(w =>
        {
            int number;

            return int.TryParse(
                w.WardNumber,
                out number)
                ? number
                : int.MaxValue;
        })
        .ToList();

            return View(model);
        }



        // ============================================================
        // CREATE - GET
        // ============================================================

        [HttpGet]
        public ActionResult Create()
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            var model = new MunicipalAssetCreateViewModel
            {
                Condition = AssetCondition.Good,
                Status = AssetStatus.Active
            };

            PopulateAssetCreateOptions(model);

            return View(model);
        }

        // ============================================================
        // CREATE - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MunicipalAssetCreateViewModel model)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            var administratorID = GetAdministratorID();

            if (!administratorID.HasValue)
                return new HttpUnauthorizedResult();

            /*
             * ============================================================
             * READ COORDINATES DIRECTLY FROM THE FORM
             * ============================================================
             *
             * Latitude and Longitude in the Create ViewModel are strings.
             *
             * The form posts:
             *
             * name="Latitude"
             * name="Longitude"
             *
             * Read those exact values and keep them as strings in the
             * ViewModel. They are converted to doubles only after
             * validation when the MunicipalAsset entity is created.
             */

            var postedLatitude =
                Request.Form["Latitude"];

            var postedLongitude =
                Request.Form["Longitude"];

            model.Latitude =
                string.IsNullOrWhiteSpace(postedLatitude)
                    ? null
                    : postedLatitude.Trim();

            model.Longitude =
                string.IsNullOrWhiteSpace(postedLongitude)
                    ? null
                    : postedLongitude.Trim();

            /*
             * Normalize user-entered values.
             */
            model.AssetType =
                model.AssetType?.Trim();

            model.AssetCategory =
                model.AssetCategory?.Trim();

            model.Description =
                model.Description?.Trim();

            model.LocationDescription =
                model.LocationDescription?.Trim();

            /*
             * ============================================================
             * VALIDATE ASSET CLASSIFICATION
             * ============================================================
             */

            var classification =
                GetAssetCategories();

            if (string.IsNullOrWhiteSpace(model.AssetType) ||
                !classification.ContainsKey(model.AssetType))
            {
                ModelState.AddModelError(
                    "AssetType",
                    "Please select a valid asset type."
                );
            }

            if (!string.IsNullOrWhiteSpace(model.AssetType) &&
                classification.ContainsKey(model.AssetType))
            {
                if (string.IsNullOrWhiteSpace(model.AssetCategory) ||
                    !classification[model.AssetType]
                        .Contains(model.AssetCategory))
                {
                    ModelState.AddModelError(
                        "AssetCategory",
                        "Please select a valid asset category for the selected asset type."
                    );
                }
            }



            /*
             * ============================================================
             * VALIDATE LOCATION
             * ============================================================
             */

            double selectedLatitude;
            double selectedLongitude;

            bool latitudeIsValid =
                double.TryParse(
                    model.Latitude,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out selectedLatitude
                );

            bool longitudeIsValid =
                double.TryParse(
                    model.Longitude,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out selectedLongitude
                );

            if (!latitudeIsValid ||
                !longitudeIsValid)
            {
                ModelState.AddModelError(
                    "",
                    "Please select the asset location on the map."
                );
            }
            else
            {
                if (selectedLatitude < -90 ||
                    selectedLatitude > 90)
                {
                    ModelState.AddModelError(
                        "Latitude",
                        "Latitude must be between -90 and 90."
                    );
                }

                if (selectedLongitude < -180 ||
                    selectedLongitude > 180)
                {
                    ModelState.AddModelError(
                        "Longitude",
                        "Longitude must be between -180 and 180."
                    );
                }

                /*
                 * 0,0 is technically a valid coordinate but is not a
                 * realistic municipal asset location for this system.
                 */
                if (Math.Abs(selectedLatitude) < 0.000001 &&
                    Math.Abs(selectedLongitude) < 0.000001)
                {
                    ModelState.AddModelError(
                        "",
                        "Please select a valid municipal asset location rather than the default map position."
                    );
                }
            }

            if (!model.WardID.HasValue)
            {
                ModelState.AddModelError(
                    "WardID",
                    "Please select the municipal ward."
                );
            }
            else
            {
                var wardExists =
                    db.Wards.Any(w =>
                        w.WardID == model.WardID.Value);

                if (!wardExists)
                {
                    ModelState.AddModelError(
                        "WardID",
                        "Please select a valid municipal ward."
                    );
                }
            }

            /*
             * ============================================================
             * VALIDATE INITIAL OPERATIONAL INFORMATION
             * ============================================================
             */

            if (!model.Condition.HasValue)
            {
                ModelState.AddModelError(
                    "Condition",
                    "Please select the asset condition."
                );
            }

            if (!model.Status.HasValue)
            {
                ModelState.AddModelError(
                    "Status",
                    "Please select the asset status."
                );
            }

            /*
             * Always repopulate dropdowns before returning the view.
             */
            PopulateAssetClassificationOptions(model);

            if (!ModelState.IsValid)
                return View(model);

            /*
             * ============================================================
             * SAVE ASSET
             * ============================================================
             */

            using (var transaction =
                db.Database.BeginTransaction())
            {
                try
                {
                    /*
                     * Temporary values are required because the final
                     * AssetCode depends on the generated AssetID.
                     */
                    var asset = new MunicipalAsset
                    {
                        AssetCode =
                            "TEMP-" +
                            Guid.NewGuid()
                                .ToString("N")
                                .Substring(0, 20),

                        AssetName =
                            "Pending Asset Registration",

                        AssetType =
                            model.AssetType,

                        AssetCategory =
                            model.AssetCategory,

                        Description =
                            model.Description,

                        Latitude =
                            selectedLatitude,

                        Longitude =
                            selectedLongitude,

                        LocationDescription = model.LocationDescription,
                        WardID = model.WardID.Value,
                        Condition = model.Condition.Value,
                        Status = model.Status.Value,

                        DateRegistered =
                            DateTime.Now,

                        CreatedByAdministratorID =
                            administratorID.Value
                    };

                    db.MunicipalAssets.Add(asset);

                    /*
                     * First save obtains the generated AssetID.
                     */
                    db.SaveChanges();

                    /*
                     * ====================================================
                     * GENERATE PERMANENT ASSET CODE
                     * ====================================================
                     */

                    asset.AssetCode =
                        $"AST-{asset.AssetID:D6}";

                    /*
                     * ====================================================
                     * GENERATE CONTROLLED ASSET NAME
                     * ====================================================
                     */

                    asset.AssetName =
                        GenerateAssetName(
                            asset.AssetCategory,
                            asset.LocationDescription,
                            asset.AssetID
                        );

                    db.SaveChanges();

                    /*
                     * ====================================================
                     * CREATE INITIAL AUDIT RECORD
                     * ====================================================
                     */

                    var history = new AssetHistory
                    {
                        AssetID =
                            asset.AssetID,

                        AdministratorID =
                            administratorID.Value,

                        ActivityType =
                            "Asset Registered",

                        Description =
                            "Municipal asset was registered in the system.",

                        ActivityDate =
                            DateTime.Now,

                        PreviousValue =
                            null,

                        NewValue =
                            BuildRegistrationHistoryValue(asset)
                    };

                    db.AssetHistories.Add(history);

                    db.SaveChanges();

                    /*
                     * ====================================================
                     * COMMIT EVERYTHING
                     * ====================================================
                     */

                    transaction.Commit();

                    TempData["SuccessMessage"] =
                        "Municipal asset registered successfully.";

                    /*
                     * ====================================================
                     * SUCCESS PAGE
                     * ====================================================
                     */

                    return RedirectToAction(
                        "SuccessAsset",
                        new
                        {
                            id = asset.AssetID
                        }
                    );
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    /*
                     * Keep technical details out of the UI while
                     * retaining them for debugging.
                     */
                    Debug.WriteLine(
                        "MunicipalAsset Create Error:"
                    );

                    Debug.WriteLine(
                        ex.ToString()
                    );

                    ModelState.AddModelError(
                        "",
                        "The municipal asset could not be registered. Please review the information and try again."
                    );

                    /*
                     * Make sure the dropdowns are still available
                     * if the save fails.
                     */
                    PopulateAssetCreateOptions(model);

                    return View(model);
                }
            }
        }

        // ============================================================
        // SUCCESS
        // ============================================================

        public ActionResult SuccessAsset(int id)
        {
            if (!IsAdministrator())
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.Unauthorized
                );
            }

            var asset = db.MunicipalAssets
                .Include(a => a.CreatedByAdministrator)
                .Include(a => a.Ward)
                .FirstOrDefault(a => a.AssetID == id);

            if (asset == null)
            {
                return HttpNotFound();
            }

            return View(asset);
        }

        // ============================================================
        // ASSET NAME GENERATION
        // ============================================================

        private string GenerateAssetName(
            string assetCategory,
            string locationDescription,
            int assetID)
        {
            var category =
                string.IsNullOrWhiteSpace(assetCategory)
                    ? "Municipal Asset"
                    : assetCategory.Trim();

            var location =
                string.IsNullOrWhiteSpace(locationDescription)
                    ? null
                    : locationDescription.Trim();

            // --------------------------------------------------------
            // Try to extract the street / road name from the
            // full geocoded address.
            // --------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(location))
            {
                var addressParts =
                    location
                        .Split(',')
                        .Select(p => p.Trim())
                        .Where(p => !string.IsNullOrWhiteSpace(p))
                        .ToList();

                if (addressParts.Any())
                {
                    var streetPart =
                        addressParts.FirstOrDefault(p =>
                            p.IndexOf("Street",
                                StringComparison.OrdinalIgnoreCase) >= 0
                            ||
                            p.IndexOf("Road",
                                StringComparison.OrdinalIgnoreCase) >= 0
                            ||
                            p.IndexOf("Avenue",
                                StringComparison.OrdinalIgnoreCase) >= 0
                            ||
                            p.IndexOf("Drive",
                                StringComparison.OrdinalIgnoreCase) >= 0
                            ||
                            p.IndexOf("Lane",
                                StringComparison.OrdinalIgnoreCase) >= 0
                            ||
                            p.IndexOf("Way",
                                StringComparison.OrdinalIgnoreCase) >= 0
                            ||
                            p.IndexOf("Boulevard",
                                StringComparison.OrdinalIgnoreCase) >= 0
                            ||
                            p.IndexOf("Crescent",
                                StringComparison.OrdinalIgnoreCase) >= 0
                            ||
                            p.IndexOf("Close",
                                StringComparison.OrdinalIgnoreCase) >= 0
                            ||
                            p.IndexOf("Place",
                                StringComparison.OrdinalIgnoreCase) >= 0
                            ||
                            p.IndexOf("Terrace",
                                StringComparison.OrdinalIgnoreCase) >= 0
                            ||
                            p.IndexOf("Drive",
                                StringComparison.OrdinalIgnoreCase) >= 0
                        );

                    if (!string.IsNullOrWhiteSpace(streetPart))
                    {
                        location = streetPart;
                    }
                    else
                    {
                        // If no obvious street type was found,
                        // use the first meaningful address component.
                        location = addressParts.First();
                    }
                }
            }

            // --------------------------------------------------------
            // Build the short, readable asset name.
            // --------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(location))
            {
                var name =
                    $"{category} – {location}";

                const int maxLength = 150;

                if (name.Length <= maxLength)
                    return name;

                var prefix =
                    $"{category} – ";

                var available =
                    maxLength - prefix.Length;

                if (available > 0)
                {
                    location =
                        location.Substring(
                            0,
                            Math.Min(
                                available,
                                location.Length
                            )
                        );

                    return prefix + location;
                }
            }

            // --------------------------------------------------------
            // Final fallback.
            // --------------------------------------------------------

            var fallback =
                $"Municipal Asset {assetID}";

            return fallback.Length <= 150
                ? fallback
                : fallback.Substring(0, 150);
        }

        // ============================================================
        // REGISTRATION HISTORY VALUE
        // ============================================================

        private string BuildRegistrationHistoryValue(
            MunicipalAsset asset)
        {
            var value =
                $"Asset Code: {asset.AssetCode}; " +
                $"Asset Name: {asset.AssetName}; " +
                $"Type: {asset.AssetType}; " +
                $"Category: {asset.AssetCategory}; " +
                $"Condition: {asset.Condition}; " +
                $"Status: {asset.Status}; " +
                $"Latitude: {asset.Latitude}; " +
                $"Longitude: {asset.Longitude}";

            return value.Length <= 2000
                ? value
                : value.Substring(0, 2000);
        }

        // ============================================================
        // SEARCH ADDRESS
        // ============================================================

        [HttpGet]
        public async Task<ActionResult> SearchAddress(
            string query)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            query = query?.Trim();

            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(
                    new
                    {
                        success = false,
                        message =
                            "Please enter an address or location to search."
                    },
                    JsonRequestBehavior.AllowGet
                );
            }

            if (query.Length > 200)
            {
                return Json(
                    new
                    {
                        success = false,
                        message =
                            "The location search is too long."
                    },
                    JsonRequestBehavior.AllowGet
                );
            }

            try
            {
                await Task.Run(
                    () => RespectNominatimRateLimit()
                );

                var encodedQuery =
                    Uri.EscapeDataString(query);

                var url =
                    "https://nominatim.openstreetmap.org/search" +
                    "?format=jsonv2" +
                    "&addressdetails=1" +
                    "&limit=5" +
                    "&countrycodes=za" +
                    "&q=" +
                    encodedQuery;

                using (var response =
                    await HttpClient.GetAsync(url))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        return Json(
                            new
                            {
                                success = false,
                                message =
                                    "The address search service is currently unavailable."
                            },
                            JsonRequestBehavior.AllowGet
                        );
                    }

                    var json =
                        await response.Content.ReadAsStringAsync();

                    return Content(
                        json,
                        "application/json"
                    );
                }
            }
            catch (TaskCanceledException)
            {
                return Json(
                    new
                    {
                        success = false,
                        message =
                            "The address search timed out. Please try again."
                    },
                    JsonRequestBehavior.AllowGet
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    "MunicipalAsset SearchAddress Error:"
                );

                Debug.WriteLine(
                    ex.ToString()
                );

                return Json(
                    new
                    {
                        success = false,
                        message =
                            "Unable to search for the requested location."
                    },
                    JsonRequestBehavior.AllowGet
                );
            }
        }

        // ============================================================
        // REVERSE GEOCODING
        // ============================================================

        [HttpGet]
        public async Task<ActionResult> ReverseGeocode(
            double? lat,
            double? lon)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (!lat.HasValue || !lon.HasValue)
            {
                return Json(
                    new
                    {
                        success = false,
                        message =
                            "Latitude and longitude are required."
                    },
                    JsonRequestBehavior.AllowGet
                );
            }

            if (lat.Value < -90 ||
                lat.Value > 90 ||
                lon.Value < -180 ||
                lon.Value > 180)
            {
                return Json(
                    new
                    {
                        success = false,
                        message =
                            "The selected coordinates are invalid."
                    },
                    JsonRequestBehavior.AllowGet
                );
            }

            if (Math.Abs(lat.Value) < 0.000001 &&
                Math.Abs(lon.Value) < 0.000001)
            {
                return Json(
                    new
                    {
                        success = false,
                        message =
                            "Please select a valid location."
                    },
                    JsonRequestBehavior.AllowGet
                );
            }

            try
            {
                await Task.Run(
                    () => RespectNominatimRateLimit()
                );

                var url =
                    "https://nominatim.openstreetmap.org/reverse" +
                    "?format=jsonv2" +
                    "&addressdetails=1" +
                    "&zoom=18" +
                    $"&lat={lat.Value.ToString(CultureInfo.InvariantCulture)}" +
                    $"&lon={lon.Value.ToString(CultureInfo.InvariantCulture)}";

                using (var response =
                    await HttpClient.GetAsync(url))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        return Json(
                            new
                            {
                                success = false,
                                message =
                                    "The location service is currently unavailable."
                            },
                            JsonRequestBehavior.AllowGet
                        );
                    }

                    var json =
                        await response.Content.ReadAsStringAsync();

                    return Content(
                        json,
                        "application/json"
                    );
                }
            }
            catch (TaskCanceledException)
            {
                return Json(
                    new
                    {
                        success = false,
                        message =
                            "The location lookup timed out. Please try again."
                    },
                    JsonRequestBehavior.AllowGet
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    "MunicipalAsset ReverseGeocode Error:"
                );

                Debug.WriteLine(
                    ex.ToString()
                );

                return Json(
                    new
                    {
                        success = false,
                        message =
                            "Unable to determine the selected location."
                    },
                    JsonRequestBehavior.AllowGet
                );
            }
        }

        // DETAILS / US73
        [HttpGet]
        public ActionResult Details(int? id)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);
            }

            var asset = db.MunicipalAssets
                .Include(a => a.Ward)
                .Include(a => a.CreatedByAdministrator)
                .Include(a => a.LastUpdatedByAdministrator)
                .Include(a => a.AssetInspections)
                .Include(a => a.MaintenanceNeeds)
                .Include(a => a.AssetRequests.Select(r => r.Request))
                .Include(a => a.AssetMaintenanceRecords)
                .Include(a => a.AssetProjects)
                .Include(a => a.AssetHistory)
                .FirstOrDefault(a => a.AssetID == id.Value);

            if (asset == null)
                return HttpNotFound();

            return View(asset);
        }

        // ============================================================
        // EDIT ASSET - US76
        // ============================================================

        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);
            }

            var asset = db.MunicipalAssets
                .AsNoTracking()
                .FirstOrDefault(a =>
                    a.AssetID == id.Value);

            if (asset == null)
                return HttpNotFound();

            var model = new MunicipalAssetEditViewModel
            {
                AssetID = asset.AssetID,
                AssetCode = asset.AssetCode,
                AssetName = asset.AssetName,
                AssetType = asset.AssetType,
                AssetCategory = asset.AssetCategory,
                Description = asset.Description,
                WardID = asset.WardID,
                LocationDescription = asset.LocationDescription,

                Latitude =
                    asset.Latitude.HasValue
                        ? asset.Latitude.Value.ToString(
                            CultureInfo.InvariantCulture)
                        : null,

                Longitude =
                    asset.Longitude.HasValue
                        ? asset.Longitude.Value.ToString(
                            CultureInfo.InvariantCulture)
                        : null,

                Condition = asset.Condition,
                Status = asset.Status
            };

            PopulateAssetEditOptions(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
    MunicipalAssetEditViewModel model)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            var administratorID = GetAdministratorID();

            if (!administratorID.HasValue)
                return new HttpUnauthorizedResult();

            /*
             * ============================================================
             * NORMALIZE INPUT
             * ============================================================
             */

            model.AssetName =
                model.AssetName?.Trim();

            model.AssetType =
                model.AssetType?.Trim();

            model.AssetCategory =
                model.AssetCategory?.Trim();

            model.Description =
                model.Description?.Trim();

            model.LocationDescription =
                model.LocationDescription?.Trim();

            model.Latitude =
                model.Latitude?.Trim();

            model.Longitude =
                model.Longitude?.Trim();

            /*
             * ============================================================
             * LOAD EXISTING ASSET
             * ============================================================
             */

            var asset = db.MunicipalAssets
                .FirstOrDefault(a =>
                    a.AssetID == model.AssetID);

            if (asset == null)
                return HttpNotFound();

            /*
             * ============================================================
             * VALIDATE CLASSIFICATION
             * ============================================================
             */

            var classification =
                GetAssetCategories();

            if (string.IsNullOrWhiteSpace(model.AssetType) ||
                !classification.ContainsKey(model.AssetType))
            {
                ModelState.AddModelError(
                    "AssetType",
                    "Please select a valid asset type."
                );
            }

            if (!string.IsNullOrWhiteSpace(model.AssetType) &&
                classification.ContainsKey(model.AssetType))
            {
                if (string.IsNullOrWhiteSpace(model.AssetCategory) ||
                    !classification[model.AssetType]
                        .Contains(model.AssetCategory))
                {
                    ModelState.AddModelError(
                        "AssetCategory",
                        "Please select a valid asset category for the selected asset type."
                    );
                }
            }

            /*
             * ============================================================
             * VALIDATE WARD
             * ============================================================
             */

            if (!db.Wards.Any(w =>
                w.WardID == model.WardID))
            {
                ModelState.AddModelError(
                    "WardID",
                    "Please select a valid municipal ward."
                );
            }

            /*
             * ============================================================
             * VALIDATE COORDINATES
             * ============================================================
             */

            double latitude;
            double longitude;

            bool latitudeValid =
                double.TryParse(
                    model.Latitude,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out latitude
                );

            bool longitudeValid =
                double.TryParse(
                    model.Longitude,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out longitude
                );

            if (!latitudeValid)
            {
                ModelState.AddModelError(
                    "Latitude",
                    "Please enter a valid latitude."
                );
            }

            if (!longitudeValid)
            {
                ModelState.AddModelError(
                    "Longitude",
                    "Please enter a valid longitude."
                );
            }

            if (latitudeValid)
            {
                if (latitude < -90 ||
                    latitude > 90)
                {
                    ModelState.AddModelError(
                        "Latitude",
                        "Latitude must be between -90 and 90."
                    );
                }
            }

            if (longitudeValid)
            {
                if (longitude < -180 ||
                    longitude > 180)
                {
                    ModelState.AddModelError(
                        "Longitude",
                        "Longitude must be between -180 and 180."
                    );
                }
            }

            if (latitudeValid &&
                longitudeValid &&
                Math.Abs(latitude) < 0.000001 &&
                Math.Abs(longitude) < 0.000001)
            {
                ModelState.AddModelError(
                    "",
                    "Please provide a valid municipal asset location."
                );
            }

            /*
             * ============================================================
             * VALIDATE CONDITION / STATUS
             * ============================================================
             */

            /*
             * Because these are non-nullable enum properties in the
             * Edit ViewModel, normal model validation handles them.
             */

            PopulateAssetEditOptions(model);

            if (!ModelState.IsValid)
                return View(model);

            /*
             * ============================================================
             * CAPTURE PREVIOUS VALUES
             * ============================================================
             */

            var changes = new List<string>();

            if (asset.AssetName != model.AssetName)
            {
                changes.Add(
                    $"Asset Name: '{asset.AssetName}' → '{model.AssetName}'"
                );
            }

            if (asset.AssetType != model.AssetType)
            {
                changes.Add(
                    $"Asset Type: '{asset.AssetType}' → '{model.AssetType}'"
                );
            }

            if (asset.AssetCategory != model.AssetCategory)
            {
                changes.Add(
                    $"Asset Category: '{asset.AssetCategory}' → '{model.AssetCategory}'"
                );
            }

            if (asset.Description != model.Description)
            {
                changes.Add(
                    "Description was updated."
                );
            }

            if (asset.WardID != model.WardID)
            {
                changes.Add(
                    $"Ward: {asset.WardID} → {model.WardID}"
                );
            }

            if (asset.LocationDescription !=
                model.LocationDescription)
            {
                changes.Add(
                    "Location description was updated."
                );
            }

            if (asset.Latitude != latitude)
            {
                changes.Add(
                    $"Latitude: {asset.Latitude} → {latitude}"
                );
            }

            if (asset.Longitude != longitude)
            {
                changes.Add(
                    $"Longitude: {asset.Longitude} → {longitude}"
                );
            }

            if (asset.Condition != model.Condition)
            {
                changes.Add(
                    $"Condition: {asset.Condition} → {model.Condition}"
                );
            }

            if (asset.Status != model.Status)
            {
                changes.Add(
                    $"Status: {asset.Status} → {model.Status}"
                );
            }

            /*
             * ============================================================
             * NOTHING CHANGED
             * ============================================================
             */

            if (!changes.Any())
            {
                TempData["InfoMessage"] =
                    "No changes were made to the asset.";

                return RedirectToAction(
                    "Details",
                    new { id = asset.AssetID }
                );
            }

            /*
             * ============================================================
             * APPLY CHANGES
             * ============================================================
             */

            asset.AssetName =
                model.AssetName;

            asset.AssetType =
                model.AssetType;

            asset.AssetCategory =
                model.AssetCategory;

            asset.Description =
                model.Description;

            asset.WardID =
                model.WardID;

            asset.LocationDescription =
                model.LocationDescription;

            asset.Latitude =
                latitude;

            asset.Longitude =
                longitude;

            asset.Condition =
                model.Condition;

            asset.Status =
                model.Status;

            asset.LastUpdatedDate =
                DateTime.Now;

            asset.LastUpdatedByAdministratorID =
                administratorID.Value;

            /*
             * ============================================================
             * AUDIT HISTORY
             * ============================================================
             */

            var history = new AssetHistory
            {
                AssetID =
                    asset.AssetID,

                AdministratorID =
                    administratorID.Value,

                ActivityType =
                    "Asset Updated",

                Description =
                    "Municipal asset information was updated by an administrator. " +
                    string.Join(" ", changes),
            
                ActivityDate =
                    DateTime.Now,

                PreviousValue =
                    "Existing asset information",

                NewValue =
                    string.Join(" ", changes)
            };

            db.AssetHistories.Add(history);

            try
            {
                db.SaveChanges();

                TempData["SuccessMessage"] =
                    "Municipal asset updated successfully.";

                return RedirectToAction(
                    "Details",
                    new { id = asset.AssetID }
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    "MunicipalAsset Edit Error:"
                );

                Debug.WriteLine(
                    ex.ToString()
                );

                ModelState.AddModelError(
                    "",
                    "The municipal asset could not be updated. Please try again."
                );

                PopulateAssetEditOptions(model);

                return View(model);
            }
        }

        // ============================================================
        // RECORD ASSET CONDITION - US77
        // ============================================================

        [HttpGet]
        public ActionResult RecordCondition(int? id)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);
            }

            var asset = db.MunicipalAssets
                .AsNoTracking()
                .Include(a => a.Ward)
                .FirstOrDefault(a =>
                    a.AssetID == id.Value);

            if (asset == null)
                return HttpNotFound();

            var model = new RecordAssetConditionViewModel
            {
                AssetID = asset.AssetID,
                AssetCode = asset.AssetCode,
                AssetName = asset.AssetName,
                AssetType = asset.AssetType,
                AssetCategory = asset.AssetCategory,

                WardName =
                    asset.Ward != null
                        ? "Ward " +
                          asset.Ward.WardNumber +
                          " – " +
                          asset.Ward.WardName
                        : "Ward not specified",

                CurrentCondition = asset.Condition,

                NewCondition = asset.Condition
            };

            return View(model);
        }

        // ============================================================
        // RECORD ASSET CONDITION - US77
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RecordCondition(
            RecordAssetConditionViewModel model)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            var administratorID = GetAdministratorID();

            if (!administratorID.HasValue)
                return new HttpUnauthorizedResult();

            model.Findings =
                model.Findings?.Trim();

            model.RecommendedAction =
                model.RecommendedAction?.Trim();


            var asset = db.MunicipalAssets
                .Include(a => a.Ward)
                .FirstOrDefault(a =>
                    a.AssetID == model.AssetID);

            if (asset == null)
                return HttpNotFound();


            // ------------------------------------------------------------
            // Required condition validation
            // ------------------------------------------------------------

            if (!model.NewCondition.HasValue)
            {
                ModelState.AddModelError(
                    "NewCondition",
                    "Please select the new asset condition."
                );
            }


            // ------------------------------------------------------------
            // Validate enum value
            // ------------------------------------------------------------

            if (model.NewCondition.HasValue &&
                !Enum.IsDefined(
                    typeof(AssetCondition),
                    model.NewCondition.Value))
            {
                ModelState.AddModelError(
                    "NewCondition",
                    "Please select a valid asset condition."
                );
            }


            // ------------------------------------------------------------
            // Findings validation
            // ------------------------------------------------------------

            if (string.IsNullOrWhiteSpace(model.Findings))
            {
                ModelState.AddModelError(
                    "Findings",
                    "Please provide the assessment findings."
                );
            }


            // ------------------------------------------------------------
            // Rebuild asset information for validation failure
            // ------------------------------------------------------------

            model.AssetCode =
                asset.AssetCode;

            model.AssetName =
                asset.AssetName;

            model.AssetType =
                asset.AssetType;

            model.AssetCategory =
                asset.AssetCategory;

            model.WardName =
                asset.Ward != null
                    ? "Ward " +
                      asset.Ward.WardNumber +
                      " – " +
                      asset.Ward.WardName
                    : "Ward not specified";

            model.CurrentCondition =
                asset.Condition;


            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // ------------------------------------------------------------
            // Check whether an actual condition change occurred
            // ------------------------------------------------------------

            var oldCondition =
                asset.Condition;

            var newCondition =
                model.NewCondition.Value;


            if (oldCondition == newCondition)
            {
                ModelState.AddModelError(
                    "NewCondition",
                    "The selected condition is the same as the current asset condition."
                );

                return View(model);
            }


            // ------------------------------------------------------------
            // Update asset
            // ------------------------------------------------------------

            asset.Condition =
                newCondition;

            asset.LastUpdatedDate =
                DateTime.Now;

            asset.LastUpdatedByAdministratorID =
                administratorID.Value;


            // ------------------------------------------------------------
            // Create asset history
            // ------------------------------------------------------------

            var history =
                new AssetHistory
                {
                    AssetID =
                        asset.AssetID,

                    AdministratorID =
                        administratorID.Value,

                    ActivityType =
                        "Condition Updated",

                    Description =
                        "Municipal asset condition was updated " +
                        $"from {oldCondition} to {newCondition}. " +
                        $"Assessment findings: {model.Findings}" +
                        (
                            string.IsNullOrWhiteSpace(
                                model.RecommendedAction)
                                ? ""
                                : $" Recommended action: {model.RecommendedAction}."
                        ),

                    ActivityDate =
                        DateTime.Now,

                    PreviousValue =
                        oldCondition.ToString(),

                    NewValue =
                        newCondition.ToString()
                };


            db.AssetHistories.Add(history);


            // ------------------------------------------------------------
            // Save
            // ------------------------------------------------------------

            try
            {
                db.SaveChanges();

                TempData["SuccessMessage"] =
                    "Asset condition was recorded successfully.";

                return RedirectToAction(
                    "Details",
                    new
                    {
                        id = asset.AssetID
                    }
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    "MunicipalAsset RecordCondition Error:"
                );

                Debug.WriteLine(
                    ex.ToString()
                );

                ModelState.AddModelError(
                    "",
                    "The asset condition could not be recorded. Please try again."
                );

                return View(model);
            }
        }

        [HttpGet]
        public ActionResult Inspect(int? id)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);
            }

            var asset = db.MunicipalAssets
                .AsNoTracking()
                .Include(a => a.Ward)
                .FirstOrDefault(a =>
                    a.AssetID == id.Value);

            if (asset == null)
                return HttpNotFound();

            var model = new InspectMunicipalAssetViewModel
            {
                AssetID = asset.AssetID,
                AssetCode = asset.AssetCode,
                AssetName = asset.AssetName,
                AssetType = asset.AssetType,
                AssetCategory = asset.AssetCategory,

                WardName =
                    asset.Ward != null
                        ? "Ward " +
                          asset.Ward.WardNumber +
                          " – " +
                          asset.Ward.WardName
                        : "Ward not specified",

                CurrentCondition = asset.Condition,

                InspectionDate = DateTime.Today,

                ConditionObserved = asset.Condition
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Inspect(
    InspectMunicipalAssetViewModel model)
        {
            if (!IsAdministrator())
                return new HttpUnauthorizedResult();

            var administratorID = GetAdministratorID();

            if (!administratorID.HasValue)
                return new HttpUnauthorizedResult();

            model.Findings =
                model.Findings?.Trim();

            model.InspectorNotes =
                model.InspectorNotes?.Trim();

            model.RecommendedAction =
                model.RecommendedAction?.Trim();

            var asset = db.MunicipalAssets
                .Include(a => a.Ward)
                .FirstOrDefault(a =>
                    a.AssetID == model.AssetID);

            if (asset == null)
                return HttpNotFound();

            /*
             * Re-populate display information from the database.
             * These values must never be trusted from the browser.
             */
            model.AssetCode = asset.AssetCode;
            model.AssetName = asset.AssetName;
            model.AssetType = asset.AssetType;
            model.AssetCategory = asset.AssetCategory;

            model.WardName =
                asset.Ward != null
                    ? "Ward " +
                      asset.Ward.WardNumber +
                      " – " +
                      asset.Ward.WardName
                    : "Ward not specified";

            model.CurrentCondition = asset.Condition;

            /*
             * Inspection date validation.
             */
            if (!model.InspectionDate.HasValue)
            {
                ModelState.AddModelError(
                    "InspectionDate",
                    "Please provide the inspection date."
                );
            }
            else if (model.InspectionDate.Value.Date > DateTime.Today)
            {
                ModelState.AddModelError(
                    "InspectionDate",
                    "Inspection date cannot be in the future."
                );
            }

            /*
             * Condition validation.
             */
            if (!model.ConditionObserved.HasValue)
            {
                ModelState.AddModelError(
                    "ConditionObserved",
                    "Please select the condition observed during inspection."
                );
            }
            else if (!Enum.IsDefined(
                typeof(AssetCondition),
                model.ConditionObserved.Value))
            {
                ModelState.AddModelError(
                    "ConditionObserved",
                    "Please select a valid asset condition."
                );
            }

            /*
             * Findings validation.
             */
            if (string.IsNullOrWhiteSpace(model.Findings))
            {
                ModelState.AddModelError(
                    "Findings",
                    "Please provide the inspection findings."
                );
            }

            /*
             * Next inspection date validation.
             */
            if (model.NextInspectionDate.HasValue &&
                model.NextInspectionDate.Value.Date < DateTime.Today)
            {
                ModelState.AddModelError(
                    "NextInspectionDate",
                    "The next inspection date cannot be in the past."
                );
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var inspectionDate =
                    model.InspectionDate.Value.Date;

                var observedCondition =
                    model.ConditionObserved.Value;

                /*
                 * Create the formal inspection record.
                 *
                 * AssetInspection.Condition is currently a string,
                 * so the enum is stored as its name.
                 */
                var inspection = new AssetInspection
                {
                    AssetID = asset.AssetID,
                    AdministratorID = administratorID.Value,
                    InspectionDate = inspectionDate,
                    Condition = observedCondition.ToString(),
                    Findings = model.Findings,
                    InspectorNotes = model.InspectorNotes,
                    RecommendedAction = model.RecommendedAction,
                    NextInspectionDate = model.NextInspectionDate
                };

                db.AssetInspections.Add(inspection);

                /*
                 * The inspection establishes when the asset
                 * was last formally inspected.
                 *
                 * It does NOT automatically overwrite
                 * MunicipalAsset.Condition.
                 */
                asset.LastInspectionDate = inspectionDate;

                asset.LastUpdatedDate = DateTime.Now;
                asset.LastUpdatedByAdministratorID =
                    administratorID.Value;

                /*
                 * Preserve the inspection in the asset history.
                 */
                var history = new AssetHistory
                {
                    AssetID = asset.AssetID,
                    AdministratorID = administratorID.Value,
                    ActivityType = "Asset Inspected",

                    Description =
                        "A formal asset inspection was completed. " +
                        $"Inspection condition: {observedCondition}. " +
                        $"Findings: {model.Findings}" +
                        (
                            string.IsNullOrWhiteSpace(
                                model.RecommendedAction)
                                ? ""
                                : $" Recommended action: {model.RecommendedAction}."
                        ),

                    ActivityDate = DateTime.Now,

                    PreviousValue =
                        asset.Condition.ToString(),

                    NewValue =
                        observedCondition.ToString()
                };

                db.AssetHistories.Add(history);

                db.SaveChanges();

                TempData["SuccessMessage"] =
                    "Asset inspection was recorded successfully.";

                return RedirectToAction(
                    "Details",
                    new
                    {
                        id = asset.AssetID
                    }
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    "MunicipalAsset Inspect Error:"
                );

                Debug.WriteLine(ex.ToString());

                ModelState.AddModelError(
                    "",
                    "The asset inspection could not be recorded. Please try again."
                );

                return View(model);
            }
        }

        // ============================================================
        // DISPOSE
        // ============================================================

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}