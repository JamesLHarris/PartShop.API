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


namespace Site_2024.Web.Api.Services
{
    public class PartService : IPartService
    {
        private IDataProvider _data;
        private readonly Site_2024.Web.Api.Configurations.StaticFileOptions _staticFileOptions;


        public PartService(IDataProvider data, IOptions<Site_2024.Web.Api.Configurations.StaticFileOptions> staticFileOptions)

        {
            _data = data;
            _staticFileOptions = staticFileOptions.Value;
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



        #endregion

        #region ---POST&PUT---

        public int Insert(PartAddRequest model, int userId)
        {
            int id = 0;
            string procName = "[dbo].[Parts_Insert]";

            _data.ExecuteNonQuery(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    model.UserId = userId; // explicitly assign
                    AddCommonPartsParams(model, col);

                    SqlParameter idOut = new SqlParameter("@Id", SqlDbType.Int);
                    idOut.Direction = ParameterDirection.Output;
                    col.Add(idOut);
                },
                returnParameters: delegate (SqlParameterCollection returnCollection)
                {
                    object oId = returnCollection["@Id"].Value;
                    int.TryParse(oId.ToString(), out id);
                });

            return id;
        }


        public void UpdatePart(PartUpdateRequest model)
        {
            string procName = "[dbo].[Parts_Update]";
            _data.ExecuteNonQuery(procName
            , inputParamMapper: delegate (SqlParameterCollection col)
            {
                AddCommonPartsParams(model, col);
                col.AddWithValue("@Id", model.Id);
            }
            , returnParameters: null);
        }

        public void UpdatePartLocation(PartLocationUpdateRequest model)
        {
            string procName = "[dbo].[Parts_UpdateLocation]";
            _data.ExecuteNonQuery(procName
            , inputParamMapper: delegate (SqlParameterCollection col)
            {
                //AddCommonPartsParams(model, col);
                col.AddWithValue("@Id", model.Id);
                col.AddWithValue("@LocationId", model.LocationId);
            }
            , returnParameters: null);
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
            part.Price = reader.GetSafeDouble(startingIndex++);
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
            part.Price = reader.GetSafeDouble(startingIndex++);
            string imagePath = reader.GetSafeString(startingIndex++);
            part.Image = string.IsNullOrEmpty(imagePath)
                ? null
                : $"{_staticFileOptions.ImageBaseUrl}{imagePath}";

            part.Available.Id = reader.GetSafeInt32(startingIndex++);
            part.Available.Status = reader.GetSafeString(startingIndex++);

            return part;
        }



        #endregion
    }
}
