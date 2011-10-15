using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CoreEngine.SceneManager;
using CoreEngine.VertexFormats;
using CoreEngine.Wavefront;
using SlimDX;
using SlimDX.Direct3D10;

namespace DxApplication.Wavefront
{
    public class Lexer
    {
        #region Operations
        /// <summary>
        /// Public method to load from an .obj extension file. 
        /// It parses the geometric data (vertexes, normal vectors, index buffer) and the material data (attribute buffer, materials), and stores it
        /// in a mesh object. 
        /// </summary>
        /// <param name="filename">File location of the .obj file</param>
        /// <param name="device">Device object to create mesh object.</param>
        /// <returns></returns>
        public bool Create(String filename, Device device)
        {
            // check if filename exists
            if (File.Exists(filename))
            {
                // clear all buffers 
                Destroy();

                // loading geometry data
                LoadGeometryFromObjFile(filename);

                // load texture files
                for (int index_material = 0; index_material < _mesh_object_file.Materials.Count; index_material++)
                {
                    if (!String.IsNullOrEmpty(_mesh_object_file.Materials[index_material].TextureString))
                    {
                        MaterialStructure current_material = _mesh_object_file.Materials[index_material];

                        // avoid loading same texture twice
                        bool found_material = false;
                        for (int x = 0; x < index_material; x++)
                        {

                            if (current_material.TextureString == _mesh_object_file.Materials[x].TextureString)
                            {
                                found_material = true;
                                current_material.ShaderResourceView = _mesh_object_file.Materials[x].ShaderResourceView;

                                // remove and add from list<struct> to save newly configured item.
                                _mesh_object_file.Materials.RemoveAt(index_material);
                                _mesh_object_file.Materials.Insert(index_material, current_material);

                                break;
                            }
                        }

                        if (!found_material)
                        {
                            // texture not found, load texture

                            string full_texture_path = current_material.TextureString;
                            if(!File.Exists(full_texture_path))
                            {
                                full_texture_path = Path.Combine(_mesh_file_directory, current_material.TextureString);
                                if (!File.Exists(full_texture_path)) throw new FileNotFoundException();
                            }

                            Texture2D texture = Texture2D.FromFile(device, full_texture_path);
                            current_material.ShaderResourceView = new ShaderResourceView(device, texture);

                            // remove and add from list<struct> to save newly configured item. 
                            _mesh_object_file.Materials.RemoveAt(index_material);
                            _mesh_object_file.Materials.Insert(index_material, current_material);
                        }
                    }
                }

                // Create the encapsulated mesh
                _mesh_object_file.Mesh = new Mesh(device, PositionNormalTextureVertex.InputElements, PositionNormalTextureVertex.InputElements[0].SemanticName, 
                    _mesh_object_file.Vertices.Count, _mesh_object_file.Indices.Count / 3, MeshFlags.Has32BitIndices);

                // set the buffer for the mesh, mesh has its own attribute, index and vertex buffer
                // storing vertex buffer in the mesh
                _mesh_object_file.Mesh.SetVertexData(0, new DataStream(_mesh_object_file.Vertices.ToArray(), true, true));
                _mesh_object_file.Vertices.Clear();

                // storing index buffer
                _mesh_object_file.Mesh.SetIndexData(new DataStream(_mesh_object_file.Indices.ToArray(), true, true), _mesh_object_file.Indices.Count);
                _mesh_object_file.Indices.Clear();

                // storing attribute buffer
                _mesh_object_file.Mesh.SetAttributeData(new DataStream(_mesh_object_file.Attributes.ToArray(), true, true));
                _mesh_object_file.Attributes.Clear();

                // reorder the vertices according to subset and optimize the mesh for this graphics card's vertex cache.
                // When rendering the mesh's triangle list the vertices will cache hit more often so it won't have to re-execute
                // the vertex shader.
                _mesh_object_file.Mesh.GenerateAdjacencyAndPointRepresentation(0.001f);
                _mesh_object_file.Mesh.Optimize(MeshOptimizeFlags.AttributeSort | MeshOptimizeFlags.VertexCache);

                _mesh_object_file.NumberAttribTableEntries = _mesh_object_file.Mesh.GetAttributeTable().Count;

                _mesh_object_file.Mesh.Commit();
            }

            return true;
        }

        /// <summary>
        /// Dispatches the first token of the .obj file. 
        /// Possible tokens: v - vertex positions, vn - vertex normals, vt - texture coordinates, 
        /// f - faces, mtlib - material file source, usemtl - material selection.
        /// </summary>
        /// <param name="splitted"></param>
        private void DispatchToken(string[] splitted)
        {
            switch (splitted[0])
            {
                case "v":
                    AddPositionVector(splitted);
                    break;

                case "vt":
                    AddTextureCoordinates(splitted);
                    break;

                case "vn":
                    AddNormalVector(splitted);
                    break;

                case "f":
                    AddFaces(splitted);
                    break;

                case "mtllib":
                    AddMeshFile(splitted);
                    break;

                case "usemtl":
                    MaterialUsage(splitted);
                    break;
                default: 
                    Console.WriteLine("Ignoring command " + splitted[0]);
                    break;
            }
        }

        /// <summary>
        /// Dispatching geometry file.
        /// Format: usemtl string
        /// Faces which are listed after this point in the file will use the current selected material.
        /// </summary>
        /// <param name="splitted"></param>
        private void MaterialUsage(string[] splitted)
        {
            if (splitted.Length >= 2)
            {
                String material_name = splitted[1];
                bool material_found = false;
                for (int i = 0; i < _mesh_object_file.Materials.Count; i++)
                {
                    if (material_name == _mesh_object_file.Materials.ElementAt(i).Name)
                    {
                        material_found = true;
                        _current_material_subset_index = i;
                        break;
                    }
                }

                if (!material_found)
                {
                    MaterialStructure new_material;
                    _current_material_subset_index = _mesh_object_file.Materials.Count;
                    _mesh_object_file.InitMaterial(out new_material);
                    new_material.Name = material_name;
                    _mesh_object_file.Materials.Add(new_material);
                }
            }
        }

        /// <summary>
        /// Dispatching geometry file. 
        /// Format: mtllib string
        /// References the MTL (material) file for this mesh. The material file contain illumination 
        /// variables and texture file names. 
        /// </summary>
        /// <param name="splitted"></param>
        private void AddMeshFile(string[] splitted)
        {
            if (splitted.Length >= 2)
            {
                _mesh_material_file_address = splitted[1];
            }
        }

        /// <summary>
        /// Dispatcher for the geometry file. 
        /// Formats: f int int int | f int/int int/int int/int | f int/int/int int/int/int int/int/int
        /// Collects the data from the temporary lists and assign them to the mesh. 
        /// Faces are stored as series of three vertices in clockwise order. Vertices are described by their 
        /// position, optional texture coordinate, and optional normal, encoded as indices into the respective
        /// temporary component list. 
        /// </summary>
        /// <param name="splitted"></param>
        private void AddFaces(string[] splitted)
        {
            if (splitted.Length >= 4)
            {
                int index_position = 0, index_texcoord, index_normal;
                int vertex_in_face, first_index;

                int[] index_helper = new int[4];
                PositionNormalTextureVertex vertex = new PositionNormalTextureVertex();

                if (splitted[1] == string.Empty)
                {
                    // more than 1 space distance
                    int old_length = splitted.Length;
                    
                    // remove additional spacings
                    List<String> tmp_space_split = new List<string>();
                    for (int i = 0; i < old_length; i++)
                    {
                        if (splitted[i] != string.Empty)
                        {
                            tmp_space_split.Add(splitted[i]);
                        }
                    }

                    splitted = tmp_space_split.ToArray();
                }

                vertex_in_face = splitted.Length - 1;
           
                for (int index_face = 0; index_face < vertex_in_face; index_face++)
                {
                    // get indices for position, texcoords and normals out of the file and get with them the values from the according normal, position or texcoords list
                    // for a single vertice
                    string[] vertice_series = splitted[1 + index_face].Split('/');
                    
                    if (vertice_series.Length >= 1)
                    {
                        // is in format: f int int int
                        index_position = Int32.Parse(vertice_series[0]);
                        vertex.Position = _tmp_positions[index_position - 1];
                    }

                    if (vertice_series.Length >= 2)
                    {
                        // format: f int/int int/int int/int
                        if (!String.IsNullOrEmpty(vertice_series[1]))
                        {
                            index_texcoord = Int32.Parse(vertice_series[1]);
                            vertex.TextureCoordinate = _tmp_tex_coords[index_texcoord - 1];
                        }
                    }

                    if (vertice_series.Length >= 3)
                    {
                        // format: int/int/int ..
                        index_normal = Int32.Parse(vertice_series[2]);
                        vertex.Normal = _tmp_normals[index_normal - 1];
                    }

                    // if a duplicate vertex doesn't exist, add this vertex to the vertices list and store the index in the indices list. 
                    // after the fileinput these data eventually become the vertex buffer and index buffer for the mesh. 
                    int index = AddVertex(index_position, vertex);

                    // faces helper for four faces -> f 1 2 3 4
                    if(vertex_in_face == 4 && index_face >= 0 && index_face < 4)
                    {
                        index_helper[index_face] = index;
                    }

                    if(index_face < 3)
                    {
                        _mesh_object_file.Indices.Add(index);
                    }
                    else
                    {
                        _mesh_object_file.Attributes.Add(_current_material_subset_index);
                        // add new trianglestrip
                        //for (int i = 2; i >= 0; i--)
                        //    _mesh_object_file.Indices.Add(index_helper[i]);
                        _mesh_object_file.Indices.Add(index_helper[0]);
                        _mesh_object_file.Indices.Add(index_helper[2]);
                        _mesh_object_file.Indices.Add(index_helper[3]);

                        _mesh_object_file.Attributes.Add(_current_material_subset_index);
                    }
                }

                if (vertex_in_face < 4)
                    _mesh_object_file.Attributes.Add(_current_material_subset_index);
            }
        }

        /// <summary>
        /// Dispatching the geometry file. 
        /// Format: vn float float float
        /// Vertex normals.
        /// </summary>
        /// <param name="splitted"></param>
        private void AddNormalVector(string[] splitted)
        {
            Vector3 normal_vector = Vector3.Zero;
            if (splitted.Length >= 4)
            {
                normal_vector = new Vector3(float.Parse(splitted[1], CultureInfo.InvariantCulture), float.Parse(splitted[2], CultureInfo.InvariantCulture), float.Parse(splitted[3], CultureInfo.InvariantCulture));
                _tmp_normals.Add(normal_vector);
            }
        }

        /// <summary>
        /// Dispatching the geometry file. 
        /// Format: vt float float
        /// Texture coordinates (u, v).
        /// </summary>
        /// <param name="splitted"></param>
        private void AddTextureCoordinates(string[] splitted)
        {
            Vector2 texture_coordinate = Vector2.Zero;
            if (splitted.Length >= 3)
            {
                texture_coordinate = new Vector2(float.Parse(splitted[1], CultureInfo.InvariantCulture), float.Parse(splitted[2], CultureInfo.InvariantCulture));
                _tmp_tex_coords.Add(texture_coordinate);
            }
        }

        /// <summary>
        /// Dispatching the geometry file. 
        /// Format: v float float float
        /// Vertex position.
        /// </summary>
        /// <param name="splitted"></param>
        private void AddPositionVector(string[] splitted)
        {
            Vector3 position_vector = Vector3.Zero;
            if (splitted.Length >= 4)
            {

                float x = 0;
                float y = 0;
                float z = 0;

                float.TryParse(splitted[1], NumberStyles.Float, CultureInfo.InvariantCulture, out x);
                float.TryParse(splitted[2], NumberStyles.Float, CultureInfo.InvariantCulture, out y);
                float.TryParse(splitted[3], NumberStyles.Float, CultureInfo.InvariantCulture, out z);

                position_vector = new Vector3(x, y, z); // in reality it is a point, therefore the 1 at the end.
                _tmp_positions.Add(position_vector);
            }
        }

        /// <summary>
        /// Parsing material out of .mtl file.
        /// The material file contains ilumination variables and texture file names. 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private bool LoadMaterialsFromMtlFile(String filename)
        {
            // file check was done before
            try
            {
                FileStream file_stream = new FileStream(filename, FileMode.Open);
                StreamReader stream_reader = new StreamReader(file_stream);
                String readed_line = stream_reader.ReadLine();
                String[] splitted_input;
                while (readed_line != null)
                {
                    // do processing, and read another line
                    if (readed_line.Contains(" "))
                    {
                        splitted_input = readed_line.Split(' ');
                        DispatchMaterialTokenn(splitted_input);
                    }

                    readed_line = stream_reader.ReadLine();
                }

                stream_reader.Close();

            }
            catch
            {
                throw new Exception("Exception on loading materials");
                // input stream exception or parse exception
                // return false
            }
            return true;
        }

        /// <summary>
        /// Reads from a given file and creates all necessary geometry data. 
        /// If in the file was another file location to material data it loads them too. 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool LoadGeometryFromObjFile(String filename)
        {
            // file exists definitely!
            FileInfo file_info = new FileInfo(filename);
            _mesh_file_address = filename;
            _mesh_file_directory = file_info.DirectoryName;

            _current_material_subset_index = 0;

            // add first subset of default material
            MaterialStructure material;
            _mesh_object_file.InitMaterial(out material);
            material.Name = "default";
            _mesh_object_file.Materials.Add(material);

            // initialize temporary helper lists
            InitializeTemporaryHelper();

            // read file input
            try
            {
                String[] splitted;
                FileStream file_stream = new FileStream(filename, FileMode.Open);
                StreamReader stream_reader = new StreamReader(file_stream);
                String readed_line = string.Empty;

                readed_line = stream_reader.ReadLine();
                while (readed_line != null)
                {
                    if (readed_line.Length > 0 && readed_line.Contains(" "))
                    {
                        splitted = readed_line.Split(' ');
                        if (splitted.Length > 0)
                        {
                            DispatchToken(splitted);
                        }
                    }
                    readed_line = stream_reader.ReadLine();
                }

                stream_reader.Close();

            }
            catch (IOException ioe)
            {
                throw new IOException("Exception in parsing a mesh .obj file", ioe);
                // return false;  
            }
            catch (Exception ex)
            {
                // catch such thing as numberformat exception and so on 
                throw new Exception("Exception in parsing a mesh .obj file", ex);
                // return false;
            }

            // if an associated material file was found, read that in as well.
            if (_mesh_material_file_address != string.Empty)
            {
                string mesh_file_location = _mesh_material_file_address;
                if (!File.Exists(mesh_file_location))
                {
                    mesh_file_location = Path.Combine(_mesh_file_directory, mesh_file_location);
                    if (!File.Exists(mesh_file_location)) throw new FileNotFoundException();
                }

                return LoadMaterialsFromMtlFile(mesh_file_location);
            }

            return true;
        }

        /// <summary>
        /// Initialization of the temporary helper lists which get assigned in the face assigning stage for the 3dmesh object.
        /// </summary>
        private static void InitializeTemporaryHelper()
        {
            if (_tmp_positions != null)
            {
                _tmp_positions.Clear();
            }
            else
            {
                _tmp_positions = new List<Vector3>();
            }

            if (_tmp_tex_coords != null)
            {
                _tmp_tex_coords.Clear();
            }
            else
            {
                _tmp_tex_coords = new List<Vector2>();
            }

            if (_tmp_normals != null)
            {
                _tmp_normals.Clear();
            }
            else
            {
                _tmp_normals = new List<Vector3>();
            }
        }

        /// <summary>
        /// Dispatches the tokens from .mtl (material) file.
        /// Possible tokens are: newmtl - defines a new material, Ka - Ambient color, Kd - Diffuse color, 
        /// Ks - Specular color, d or Tr - Transparency, Ns - Shininess of an object (specular power), 
        /// illum - Illumination model (1 = specular disabled, 2 = specular enabled)
        /// map_Kd - Texture map. 
        /// </summary>
        /// <param name="splitted_input"></param>
        private void DispatchMaterialTokenn(string[] splitted_input)
        {
            MaterialStructure material;
            switch (splitted_input[0])
            {
                case "newmtl":
                    String material_name = splitted_input[1];

                    for (int i = 0; i < _mesh_object_file.Materials.Count; i++)
                    {
                        if (_mesh_object_file.Materials.ElementAt(i).Name == material_name)
                        {
                            _current_material_index = i;
                            break;
                        }
                    }

                    break;

                case "Ka":
                    if (splitted_input.Length >= 4)
                    {
                        material = _mesh_object_file.Materials.ElementAt(_current_material_index);
                        _mesh_object_file.Materials.RemoveAt(_current_material_index);

                        material.Ambient = new Vector3(float.Parse(splitted_input[1], CultureInfo.InvariantCulture), float.Parse(splitted_input[2], CultureInfo.InvariantCulture),
                            float.Parse(splitted_input[3], CultureInfo.InvariantCulture));

                        _mesh_object_file.Materials.Insert(_current_material_index, material);
                    }
                    break;

                case "Kd":
                    if (splitted_input.Length >= 4)
                    {
                        material = _mesh_object_file.Materials.ElementAt(_current_material_index);
                        _mesh_object_file.Materials.RemoveAt(_current_material_index);

                        material.Diffuse = new Vector3(float.Parse(splitted_input[1], CultureInfo.InvariantCulture), float.Parse(splitted_input[2], CultureInfo.InvariantCulture),
                            float.Parse(splitted_input[3], CultureInfo.InvariantCulture));

                        _mesh_object_file.Materials.Insert(_current_material_index, material);
                    }
                    break;

                case "Ks":
                    if (splitted_input.Length >= 4)
                    {
                        material = _mesh_object_file.Materials.ElementAt(_current_material_index);
                        _mesh_object_file.Materials.RemoveAt(_current_material_index);

                        material.Specular = new Vector3(float.Parse(splitted_input[1], CultureInfo.InvariantCulture), float.Parse(splitted_input[2], CultureInfo.InvariantCulture),
                            float.Parse(splitted_input[3], CultureInfo.InvariantCulture));

                        _mesh_object_file.Materials.Insert(_current_material_index, material);
                    }
                    break;

                case "d":
                    if (splitted_input.Length >= 4)
                    {
                        material = _mesh_object_file.Materials.ElementAt(_current_material_index);
                        _mesh_object_file.Materials.RemoveAt(_current_material_index);

                        material.Alpha = float.Parse(splitted_input[3], CultureInfo.InvariantCulture);

                        _mesh_object_file.Materials.Insert(_current_material_index, material);
                    }
                    else if (splitted_input.Length == 2)
                    {
                        material = _mesh_object_file.Materials.ElementAt(_current_material_index);
                        _mesh_object_file.Materials.RemoveAt(_current_material_index);

                        material.Alpha = float.Parse(splitted_input[1], CultureInfo.InvariantCulture);

                        _mesh_object_file.Materials.Insert(_current_material_index, material);
                    }
                    break;

                case "Ns":
                    if (splitted_input.Length >= 2)
                    {
                        material = _mesh_object_file.Materials.ElementAt(_current_material_index);
                        _mesh_object_file.Materials.RemoveAt(_current_material_index);

                        material.Shininess = float.Parse(splitted_input[1], CultureInfo.InvariantCulture);

                        _mesh_object_file.Materials.Insert(_current_material_index, material);
                    }
                    break;

                case "illum":
                    if (splitted_input.Length >= 2)
                    {
                        material = _mesh_object_file.Materials.ElementAt(_current_material_index);
                        _mesh_object_file.Materials.RemoveAt(_current_material_index);

                        int illumination = Int32.Parse(splitted_input[1]);
                        material.bSpecular = (illumination == 2);

                        _mesh_object_file.Materials.Insert(_current_material_index, material);
                    }
                    break;

                case "map_Kd":
                    if (splitted_input.Length >= 2)
                    {
                        material = _mesh_object_file.Materials.ElementAt(_current_material_index);
                        _mesh_object_file.Materials.RemoveAt(_current_material_index);

                        material.TextureString = splitted_input[1];

                        _mesh_object_file.Materials.Insert(_current_material_index, material);
                    }
                    break;
            }
        }
        
        /// <summary>
        /// If the incoming vertex does already exist in the vertex collection of the meshObject it returns the index of the already existing item.
        /// Else it adds the incoming vertex to the collection and returns its current index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="vertex"></param>
        /// <returns></returns>
        private int AddVertex(int index, PositionNormalTextureVertex vertex)
        {
            // TODO PERFORMANCE LEAK for large meshes
            //if (_mesh_object_file.Vertices.Contains(vertex))
            //{
            //    return _mesh_object_file.Vertices.IndexOf(vertex);
            //}
            //else
            {
                _mesh_object_file.Vertices.Add(vertex);
                return _mesh_object_file.Vertices.Count - 1;
            }
        }

        /// <summary>
        /// Resets all object variables. 
        /// </summary>
        public void Destroy()
        {
            _mesh_object_file.Initialize();
            _mesh_file_address = string.Empty;
            _mesh_material_file_address = string.Empty;

            if (_tmp_positions != null)
            {
                _tmp_positions.Clear();
                _tmp_positions = null;
            }

            if (_tmp_normals != null)
            {
                _tmp_normals.Clear();
                _tmp_normals = null;
            }

            if (_tmp_tex_coords != null)
            {
                _tmp_tex_coords.Clear();
                _tmp_tex_coords = null;
            }
        }

        /// <summary>
        /// Returns the filename of the .obj file.
        /// </summary>
        /// <returns></returns>
        public String GetMeshObjFileAddress()
        {
            return _mesh_file_address;
        }

        /// <summary>
        /// Returns after the creation process the generated Object from the .obj file.
        /// </summary>
        /// <returns></returns>
        public MeshObj GetMeshObj()
        {
            return _mesh_object_file;
        } 

      
        #endregion

        #region Attributes
        private MeshObj _mesh_object_file;
        private String _mesh_file_address;
        private String _mesh_file_directory = string.Empty;
        private String _mesh_material_file_address = string.Empty;
        
        private int _current_material_subset_index = 0;
        private int _current_material_index = -1;

        private static List<Vector3> _tmp_positions;
        private static List<Vector3> _tmp_normals;
        private static List<Vector2> _tmp_tex_coords;

        #endregion
    }
}
