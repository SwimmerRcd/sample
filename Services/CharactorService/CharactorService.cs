using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using sample.Dtos.Charactor;
using sample.Models;
using static Utils.OracleHelper;
using static System.Console;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace sample.Services.CharactorService
{
    public class CharactorService : ICharactorService
    {
        private readonly IMapper _mapper;
        public CharactorService(IMapper mapper)
        {
            _mapper = mapper;
        }
        private static List<Charactor> charactors = new List<Charactor>{
      new Charactor(),
      new Charactor{Id=1, Name="Sam"}
    };
        public async Task<ServiceResponse<List<GetCharactorDto>>> AddCharactor(AddCharactorDto newCharactor)
        {
            ServiceResponse<List<GetCharactorDto>> serviceResponse = new ServiceResponse<List<GetCharactorDto>>();
            // Charactor charactor = _mapper.Map<Charactor>(newCharactor);
            // charactor.Id = charactors.Max(c => c.Id) + 1;
            // charactors.Add(charactor);
            string cls = string.Format("{0:d2}", Convert.ToInt64(newCharactor.Class));
            string insertSql = $@"insert into charactor 
                                      (id, name, hitpoints, 
                                       strength, defense, intelligence, class)
                                values  
                                      (SEQ_CHARACTORS.NEXTVAL, '{newCharactor.Name}', {newCharactor.HitPoints}, 
                                       {newCharactor.Strength}, {newCharactor.Defense}, {newCharactor.Intelligence}, 
                                       '{cls}')";
            // OracleParameter[] param = new OracleParameter[]
            // {
            //     AddInputParameter("viId", 1, OracleDbType.Int64),
            //     AddInputParameter("viName", newCharactor.Name, OracleDbType.Varchar2, 50),
            //     AddInputParameter("viHitpoints", newCharactor.HitPoints, OracleDbType.Int64),
            //     AddInputParameter("viStrength", newCharactor.Strength, OracleDbType.Int64),
            //     AddInputParameter("viDefense", newCharactor.Defense, OracleDbType.Int64),
            //     AddInputParameter("viIntelligence", newCharactor.Intelligence, OracleDbType.Int64),
            //     AddInputParameter("viRpgclass", string.Format("{0:d2}", Convert.ToInt64(newCharactor.Class)), OracleDbType.Varchar2, 2)
            // };
            OracleConnection conn = OpenConn();
            int affectRows = 0;
            //when
            try
            {
                affectRows = ExecuteSql(insertSql, null, conn);
                serviceResponse = await GetAllCharactors();
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message + "; " + newCharactor.Class;
            }
            //then
            finally
            {
                conn.Close();
            }

            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharactorDto>>> GetAllCharactors()
        {
            ServiceResponse<List<GetCharactorDto>> serviceResponse = new ServiceResponse<List<GetCharactorDto>>();
            string querySql = @"select id, name, hitpoints, strength, defense, intelligence, class
                                  from charactor
                                 where rownum < 100";
            OracleConnection conn = OpenConn();
            try
            {
                DataTable dt = ReadTable(querySql, null, conn);
                List<Charactor> charactors = DataTableToList<Charactor>(dt);
                serviceResponse.Data = (charactors.Select(c => _mapper.Map<GetCharactorDto>(c))).ToList();
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Success = false;
            }
            finally
            {
                conn.Close();
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharactorDto>> GetCharactorById(int id)
        {
            ServiceResponse<GetCharactorDto> serviceResponse = new ServiceResponse<GetCharactorDto>();
            string querySql = $@"select id, name, hitpoints, strength, defense, intelligence, class
                                   from charactor
                                  where id = {id}";
            // serviceResponse.Data = _mapper.Map<GetCharactorDto>(charactors.FirstOrDefault(c => c.Id == id));
            OracleConnection conn = OpenConn();
            try
            {
                DataTable dt = ReadTable(querySql, null, conn);
                List<Charactor> charactors = DataTableToList<Charactor>(dt);
                // serviceResponse.Data = (charactors.Select(c => _mapper.Map<GetCharactorDto>(c))).ToList();
                serviceResponse.Data = _mapper.Map<GetCharactorDto>(charactors[0]);
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Success = false;
            }
            finally
            {
                conn.Close();
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharactorDto>> UpdateCharactor(UpdateCharactorDto updateCharactorDto)
        {
            ServiceResponse<GetCharactorDto> serviceResponse = new ServiceResponse<GetCharactorDto>();
            string cls = string.Format("{0:d2}", Convert.ToInt64(updateCharactorDto.Class));
            string updateSql = $@"update charactor
                                     set name         = '{updateCharactorDto.Name}', 
                                         hitpoints    = {updateCharactorDto.HitPoints}, 
                                         strength     = {updateCharactorDto.Strength}, 
                                         defense      = {updateCharactorDto.Defense}, 
                                         intelligence = {updateCharactorDto.Intelligence}, 
                                         class        = '{cls}'
                                  where id = {updateCharactorDto.Id}";
            OracleConnection conn = OpenConn();
            int affectRows = 0;
            try
            {
                affectRows = ExecuteSql(updateSql, null, conn);
                if (affectRows < 1)
                {
                    throw new Exception($@"更新失败，未找到id为{updateCharactorDto.Id}的记录！");
                }

                serviceResponse = await GetCharactorById(updateCharactorDto.Id);
                // Charactor charactor = charactors.FirstOrDefault(c => c.Id == updateCharactorDto.Id);
                // charactor.Name = updateCharactorDto.Name;
                // charactor.HitPoints = updateCharactorDto.HitPoints;
                // charactor.Strength = updateCharactorDto.Strength;
                // charactor.Defense = updateCharactorDto.Defense;
                // charactor.Intelligence = updateCharactorDto.Intelligence;
                // charactor.Class = updateCharactorDto.Class;
                //serviceResponse.Data = _mapper.Map<GetCharactorDto>(charactor);
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            finally
            {
                conn.Close();
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharactorDto>>> DeleteCharactor(int id)
        {
            ServiceResponse<List<GetCharactorDto>> serviceResponse = new ServiceResponse<List<GetCharactorDto>>();
            string deleteSql = $@"delete charactor where id = {id}";
            OracleConnection conn = OpenConn();
            int affectRows = 0;
            try
            {
                affectRows = ExecuteSql(deleteSql, null, conn);
                if (affectRows < 1)
                {
                    throw new Exception($@"删除失败，未找到id为{id}的记录！");
                }
                serviceResponse = await GetAllCharactors();
                // Charactor charactor = charactors.First(c => c.Id == id);
                // charactors.Remove(charactor);
                // serviceResponse.Data = (charactors.Select(c => _mapper.Map<GetCharactorDto>(c))).ToList();
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            finally{
                conn.Close();
            }
            return serviceResponse;
        }
    }
}