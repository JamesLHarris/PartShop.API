using Site_2024.Web.Api.Interfaces;
using Site_2024.Web.Api.Models;
using System.Data;
using System.Linq;
using Site_2024.Web.Api.Extensions;
using System.Data.SqlClient;
using Site_2024.Web.Api.Constructors;
using Site_2024.Web.Api.Requests;
using Microsoft.Extensions.Options;
using Site_2024.Web.Api.Configurations;
using Microsoft.Extensions.Logging;


namespace Site_2024.Web.Api.Services
{
    public class PartService : IPartService
    {
        private IDataProvider _data;
        private readonly Site_2024.Web.Api.Configurations.StaticFileOptions _staticFileOptions;
        private readonly ILogger _logger;

        public PartService(ILogger<PartService> logger,IDataProvider data, IOptions<Site_2024.Web.Api.Configurations.StaticFileOptions> staticFileOptions)

        {
            _data = data;
            _staticFileOptions = staticFileOptions.Value;
            _logger = logger;
        }

        #region ---GET---
        public Part GetPartById(int id)
        {
            string procName = "[dbo].[Parts_GetById]";
            Part? part = null;

            _data.ExecuteCmd(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@Id", id);
                }, delegate (IDataReader reader, short set)
                {
                    int startingIndex = 0;
                    part = MapSinglePart(reader, ref startingIndex);
                });

            return part;
        }

        public Part GetPartByIdCustomer(int id)
        {
            string procName = "[dbo].[Parts_GetByIdCustomer]";
            Part? part = null;

            _data.ExecuteCmd(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@Id", id);
                }, delegate (IDataReader reader, short set)
                {
                    int startingIndex = 0;
                    part = MapSinglePartForCustomer(reader, ref startingIndex);
                });

            return part;
        }

        public Paged<Part> GetPartsPaginated(int pageIndex, int pageSize)
        {
            string procName = "[dbo].[Parts_GetAllPaginated]";

            Paged<Part> pagedList = null;
            List<Part> list = null;
            int totalCount = 0;

            _data.ExecuteCmd(
                procName,
                inputParamMapper: delegate (SqlParameterCollection coll)
                {
                    coll.AddWithValue("@PageIndex", pageIndex);
                    coll.AddWithValue("@PageSize", pageSize);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int i = 0;
                    Part part = MapSinglePart(reader, ref i);

                    if (totalCount == 0)
                    {
                        totalCount = reader.GetSafeInt32(i++);
                    }
                    if (list == null)
                    {
                        list = new List<Part>();
                    }
                    list.Add(part);
                });
            if (list != null)
            {
                pagedList = new Paged<Part>(list, pageIndex, pageSize, totalCount);
            }
            return pagedList;
        }

        public Paged<Part> GetAvailablePaginated(int pageIndex, int pageSize, int availableId)
        {
            string procName = "[dbo].[Parts_GetAvailablePaginated]";

            Paged<Part> pagedList = null;
            List<Part> list = null;
            int totalCount = 0;

            _data.ExecuteCmd(
                procName,
                inputParamMapper: delegate (SqlParameterCollection coll)
                {
                    coll.AddWithValue("@PageIndex", pageIndex);
                    coll.AddWithValue("@PageSize", pageSize);
                    coll.AddWithValue("@AvailableId", availableId);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int i = 0;
                    Part part = MapSinglePart(reader, ref i);

                    if (totalCount == 0)
                    {
                        totalCount = reader.GetSafeInt32(i++);
                    }
                    if (list == null)
                    {
                        list = new List<Part>();
                    }
                    list.Add(part);
                });
            if (list != null)
            {
                pagedList = new Paged<Part>(list, pageIndex, pageSize, totalCount);
            }
            return pagedList;
        }
        public Paged<Part> GetAvailablePaginatedForCustomer(int pageIndex, int pageSize, int availableId)
        {
            string procName = "[dbo].[Parts_GetAvailablePaginatedCustomer]";

            Paged<Part> pagedList = null;
            List<Part> list = null;
            int totalCount = 0;

            _data.ExecuteCmd(
                procName,
                inputParamMapper: delegate (SqlParameterCollection coll)
                {
                    coll.AddWithValue("@PageIndex", pageIndex);
                    coll.AddWithValue("@PageSize", pageSize);
                    coll.AddWithValue("@AvailableId", availableId);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int i = 0;
                    Part part = MapSinglePartForCustomer(reader, ref i);

                    if (totalCount == 0)
                    {
                        totalCount = reader.GetSafeInt32(i++);
                    }
                    if (list == null)
                    {
                        list = new List<Part>();
                    }
                    list.Add(part);
                });
            if (list != null)
            {
                pagedList = new Paged<Part>(list, pageIndex, pageSize, totalCount);
            }
            return pagedList;
        }

        public Paged<Part> GetByModelPaginated(int pageIndex, int pageSize, int modelId)
        {
            string procName = "[dbo].[Parts_GetByModelPaginated]";
            List<Part> list = null;
            int totalCount = 0;

            _data.ExecuteCmd(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@PageIndex", pageIndex);
                    col.AddWithValue("@PageSize", pageSize);
                    col.AddWithValue("@ModelId", modelId);
                },
                singleRecordMapper: (reader, set) =>
                {
                    int i = 0;
                    Part part = MapSinglePart(reader, ref i);

                    if (totalCount == 0)
                    {
                        totalCount = reader.GetSafeInt32(i); // Read TotalCount
                    }

                    list ??= new List<Part>();
                    list.Add(part);
                }
           );

            return list == null ? null : new Paged<Part>(list, pageIndex, pageSize, totalCount);
        }

        public Paged<Part> GetByModelPaginatedCustomer(int pageIndex, int pageSize, int modelId)
        {
            string procName = "[dbo].[Parts_GetByModelPaginatedCustomer]";
            List<Part> list = null;
            int totalCount = 0;

            _data.ExecuteCmd(
                procName,
                inputParamMapper: col =>
                {
                    col.AddWithValue("@PageIndex", pageIndex);
                    col.AddWithValue("@PageSize", pageSize);
                    col.AddWithValue("@ModelId", modelId);
                },
                singleRecordMapper: (reader, set) =>
                {
                    int i = 0;
                    Part part = MapSinglePartForCustomer(reader, ref i);

                    if (totalCount == 0)
                    {
                        totalCount = reader.GetSafeInt32(i); // Read TotalCount
                    }

                    list ??= new List<Part>();
                    list.Add(part);
                }
            );

            return list == null ? null : new Paged<Part>(list, pageIndex, pageSize, totalCount);
        }

        public Paged<Part> GetByCategoryPaginated(int pageIndex, int pageSize, int categoryId)
        {
            string procName = "[dbo].[Parts_GetByCatagoryPaginated]";
            List<Part> list = null;
            int totalCount = 0;

            _data.ExecuteCmd(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@PageIndex", pageIndex);
                    col.AddWithValue("@PageSize", pageSize);
                    col.AddWithValue("@catagoryId", categoryId);
                },
                singleRecordMapper: (reader, set) =>
                {
                    int i = 0;
                    Part part = MapSinglePart(reader, ref i);

                    if (totalCount == 0)
                    {
                        totalCount = reader.GetSafeInt32(i); // Read TotalCount
                    }

                    list ??= new List<Part>();
                    list.Add(part);
                }
           );

            return list == null ? null : new Paged<Part>(list, pageIndex, pageSize, totalCount);
        }

        public Paged<Part> GetByCategoryPaginatedCustomer(int pageIndex, int pageSize, int categoryId)
        {
            string procName = "[dbo].[Parts_GetByCatagoryPaginatedCustomer]";
            List<Part> list = null;
            int totalCount = 0;

            _data.ExecuteCmd(
                procName,
                inputParamMapper: col =>
                {
                    col.AddWithValue("@PageIndex", pageIndex);
                    col.AddWithValue("@PageSize", pageSize);
                    col.AddWithValue("@catagoryId", categoryId);
                },
                singleRecordMapper: (reader, set) =>
                {
                    int i = 0;
                    Part part = MapSinglePartForCustomer(reader, ref i);

                    if (totalCount == 0)
                    {
                        totalCount = reader.GetSafeInt32(i); // Read TotalCount
                    }

                    list ??= new List<Part>();
                    list.Add(part);
                }
            );

            return list == null ? null : new Paged<Part>(list, pageIndex, pageSize, totalCount);
        }

        public List<PartSearchResult> Search(PartSearchRequest model)
        {
            string procName = "[dbo].[Parts_Search]";
            List<PartSearchResult> list = null;

            _data.ExecuteCmd(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@q", (object?)model.q ?? DBNull.Value);
                    col.AddWithValue("@CatagoryId", (object?)model.CatagoryId ?? DBNull.Value);
                    col.AddWithValue("@MakeId", (object?)model.MakeId ?? DBNull.Value);
                    col.AddWithValue("@ModelId", (object?)model.ModelId ?? DBNull.Value);
                    col.AddWithValue("@YearMin", (object?)model.YearMin ?? DBNull.Value);
                    col.AddWithValue("@YearMax", (object?)model.YearMax ?? DBNull.Value);
                    col.AddWithValue("@AvailableId", (object?)model.AvailableId ?? DBNull.Value);
                    col.AddWithValue("@PriceMin", (object?)model.PriceMin ?? DBNull.Value);
                    col.AddWithValue("@PriceMax", (object?)model.PriceMax ?? DBNull.Value);
                    col.AddWithValue("@Rusted", (object?)model.Rusted ?? DBNull.Value);
                    col.AddWithValue("@Tested", (object?)model.Tested ?? DBNull.Value);
                    col.AddWithValue("@SiteId", (object?)model.SiteId ?? DBNull.Value);
                    col.AddWithValue("@BoxId", (object?)model.BoxId ?? DBNull.Value);
                    col.AddWithValue("@MaxRows", model.MaxRows);
                    col.AddWithValue("@CustomerView", model.CustomerView);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int i = 0;
                    PartSearchResult p = MapPart(reader, ref i);
                    list ??= new List<PartSearchResult>();
                    list.Add(p);
                });

            return list ?? new List<PartSearchResult>();
        }

        #endregion

        #region ---POST&PUT---

        public int Insert(PartAddRequest model, int userId)
        {
            int id = 0;
            string procName = "[dbo].[Parts_Insert]";

            _data.ExecuteCmd(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    model.UserId = userId;
                    AddCommonPartsParams(model, col);

                    // REQUIRED because proc signature has: @Id INT OUTPUT
                    SqlParameter idOut = new SqlParameter("@Id", SqlDbType.Int);
                    idOut.Direction = ParameterDirection.Output;
                    col.Add(idOut);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    // Comes from: SELECT @Id AS Id;
                    if (!reader.IsDBNull(0))
                    {
                        id = reader.GetInt32(0);
                    }
                });

            return id;
        }

        public void PatchPart(int id, PartPatchRequest model, int userId)
        {
            const string procName = "[dbo].[Parts_UpdatePartial]";

            _data.ExecuteNonQuery(procName, col =>
            {
                col.Add("@Id", SqlDbType.Int).Value = id;

                var pPrice = col.Add("@Price", SqlDbType.Decimal);
                pPrice.Precision = 18;
                pPrice.Scale = 2;
                pPrice.Value = (object?)model.Price ?? DBNull.Value;

                col.Add("@AvailableId", SqlDbType.Int).Value = (object?)model.AvailableId ?? DBNull.Value;
                col.Add("@Rusted", SqlDbType.Bit).Value = (object?)model.Rusted ?? DBNull.Value;
                col.Add("@Tested", SqlDbType.Bit).Value = (object?)model.Tested ?? DBNull.Value;
                col.Add("@Description", SqlDbType.NVarChar, 4000).Value = (object?)model.Description ?? DBNull.Value;
                col.Add("@Image", SqlDbType.NVarChar, 260).Value = (object?)model.Image ?? DBNull.Value;
                col.Add("@LocationId", SqlDbType.Int).Value = (object?)model.LocationId ?? DBNull.Value;

                // Critical: always set LastMovedBy from server
                col.Add("@LastMovedBy", SqlDbType.Int).Value = userId;
            });
        }

        #endregion

        #region ---DELETE---
        public void DeletePart(int id)
        {
            string procName = "[dbo].[Parts_Delete]";
            _data.ExecuteNonQuery(procName
            , inputParamMapper: delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Id", id);
            }
            , returnParameters: null);
        }
        #endregion

        #region ---MAPPER---

        private static void AddCommonPartsParams(PartAddRequest model, SqlParameterCollection col)
        {
            col.AddWithValue("@name", model.Name);
            col.AddWithValue("@makeId", model.MakeId);
            col.AddWithValue("@year", model.Year);
            col.AddWithValue("@partnumber", model.PartNumber);
            col.AddWithValue("@catagoryId", model.CatagoryId);
            col.AddWithValue("@rusted", model.Rusted);
            col.AddWithValue("@tested", model.Tested);
            col.AddWithValue("@description", model.Description);
            col.AddWithValue("@price", model.Price);
            col.AddWithValue("@locationId", model.LocationId);
            col.AddWithValue("@image", model.Image);
            col.AddWithValue("@availableId", model.AvailableId);
            col.AddWithValue("@lastmovedby", model.UserId);

        }


        private Part MapSinglePart(IDataReader reader, ref int startingIndex)
        {
            Part part = new Part();
            part.Catagory = new Catagory();
            part.Make = new Make();
            part.Make.Model = new Model();
            part.Location = new Location();
            part.Location.Site = new Site();
            part.Location.Area = new Area();
            part.Location.Aisle = new Aisle();
            part.Location.Shelf = new Shelf();
            part.Location.Section = new Section();
            part.Location.Box = new Box();
            part.Available = new Available();
            part.User = new Models.User.User();

            part.Id = reader.GetSafeInt32(startingIndex++);
            part.Name = reader.GetSafeString(startingIndex++);
            part.Catagory.Id = reader.GetSafeInt32(startingIndex++);
            part.Catagory.Name = reader.GetSafeString(startingIndex++);
            part.Make.Id = reader.GetSafeInt32(startingIndex++);
            part.Make.Company = reader.GetSafeString(startingIndex++);
            part.Make.Model.Id = reader.GetSafeInt32(startingIndex++);
            part.Make.Model.Name = reader.GetSafeString(startingIndex++);
            part.Year = reader.GetSafeInt32(startingIndex++);
            part.PartNumber = reader.GetSafeString(startingIndex++);
            part.Rusted = reader.GetSafeBool(startingIndex++);
            part.Tested = reader.GetSafeBool(startingIndex++);
            part.Description = reader.GetSafeString(startingIndex++);
            part.Price = reader.GetSafeDecimal(startingIndex++);
            part.Location.Id = reader.GetSafeInt32(startingIndex++);
            part.Location.Site.Id = reader.GetSafeInt32(startingIndex++);
            part.Location.Site.Name = reader.GetSafeString(startingIndex++);
            part.Location.Area.Id = reader.GetSafeInt32(startingIndex++);
            part.Location.Area.Name = reader.GetSafeString(startingIndex++);
            part.Location.Aisle.Id = reader.GetSafeInt32(startingIndex++);
            part.Location.Aisle.Name = reader.GetSafeString(startingIndex++);
            part.Location.Shelf.Id = reader.GetSafeInt32( startingIndex++);
            part.Location.Shelf.Name = reader.GetSafeString( startingIndex++);
            part.Location.Section.Id = reader.GetSafeInt32(startingIndex++);
            part.Location.Section.Name = reader.GetSafeString(startingIndex++);
            part.Location.Box.Id = reader.GetSafeInt32( startingIndex++);
            part.Location.Box.Name = reader.GetSafeString(startingIndex++);
            string imagePath = reader.GetSafeString(startingIndex++);
            part.Image = string.IsNullOrEmpty(imagePath)
                ? null
                : $"{_staticFileOptions.ImageBaseUrl}{imagePath}";

            part.Available.Id = reader.GetSafeInt32(startingIndex++);
            part.Available.Status = reader.GetSafeString(startingIndex++);
            part.DateCreated = reader.GetSafeDateTime(startingIndex++);
            part.DateModified = reader.GetSafeDateTime(startingIndex++);
            part.User.Id = reader.GetSafeInt32(startingIndex++);
            part.User.Name = reader.GetSafeString(startingIndex++);

            return part;
        }

        private Part MapSinglePartForCustomer(IDataReader reader, ref int startingIndex)
        {
            Part part = new Part();
            part.Catagory = new Catagory();
            part.Make = new Make();
            part.Make.Model = new Model();
            part.Available = new Available();

            part.Id = reader.GetSafeInt32(startingIndex++);
            part.Name = reader.GetSafeString(startingIndex++);
            part.Catagory.Id = reader.GetSafeInt32(startingIndex++);
            part.Catagory.Name = reader.GetSafeString(startingIndex++);
            part.Make.Id = reader.GetSafeInt32(startingIndex++);
            part.Make.Company = reader.GetSafeString(startingIndex++);
            part.Make.Model.Id = reader.GetSafeInt32(startingIndex++);
            part.Make.Model.Name = reader.GetSafeString(startingIndex++);
            part.Year = reader.GetSafeInt32(startingIndex++);
            part.PartNumber = reader.GetSafeString(startingIndex++);
            part.Rusted = reader.GetSafeBool(startingIndex++);
            part.Tested = reader.GetSafeBool(startingIndex++);
            part.Description = reader.GetSafeString(startingIndex++);
            part.Price = reader.GetSafeDecimal(startingIndex++);
            string imagePath = reader.GetSafeString(startingIndex++);
            part.Image = string.IsNullOrEmpty(imagePath)
                ? null
                : $"{_staticFileOptions.ImageBaseUrl}{imagePath}";

            part.Available.Id = reader.GetSafeInt32(startingIndex++);
            part.Available.Status = reader.GetSafeString(startingIndex++);

            return part;
        }

        private static PartSearchResult MapPart(IDataReader reader, ref int startingIndex)
        {
            // Using your GetSafe* extensions to avoid null exceptions
            PartSearchResult p = new PartSearchResult();

            p.Id = reader.GetSafeInt32(startingIndex++);
            p.Name = reader.GetSafeString(startingIndex++);
            p.CatagoryId = reader.GetSafeInt32(startingIndex++);
            p.CatagoryName = reader.GetSafeString(startingIndex++);
            p.MakeId = reader.GetSafeInt32(startingIndex++);
            p.MakeName = reader.GetString(startingIndex++);
            p.ModelId = reader.GetSafeInt32(startingIndex++);
            p.ModelName = reader.GetSafeString(startingIndex++);
            p.Year = reader.GetSafeInt32(startingIndex++);
            p.PartNumber = reader.GetSafeString(startingIndex++);
            p.Rusted = reader.GetSafeBool(startingIndex++);
            p.Tested = reader.GetSafeBool(startingIndex++);
            p.Description = reader.GetSafeString(startingIndex++);
            p.Price = reader.GetSafeDecimal(startingIndex++);
            p.Image = reader.GetSafeString(startingIndex++);
            p.AvailableId = reader.GetSafeInt32(startingIndex++);
            p.AvailableStatus = reader.GetSafeString(startingIndex++);
            p.SiteId = reader.GetSafeInt32(startingIndex++);
            p.SiteName = reader.GetSafeString(startingIndex++);
            p.BoxId = reader.GetSafeInt32(startingIndex++);
            p.BoxName = reader.GetSafeString(startingIndex++);
            p.DateCreated = reader.GetSafeDateTime(startingIndex++);
            p.DateModified = reader.GetSafeDateTime(startingIndex++);

            return p;
        }

        #endregion
    }
}
