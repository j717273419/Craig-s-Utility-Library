﻿/*
Copyright (c) 2014 <a href="http://www.gutgames.com">James Craig</a>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.*/

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using Utilities.DataTypes;
using Utilities.DataTypes.Caching.Interfaces;
using Utilities.ORM.Manager.Aspect.Interfaces;
using Utilities.ORM.Manager.Mapper.Interfaces;
using Utilities.ORM.Manager.QueryProvider.Interfaces;
using Utilities.ORM.Manager.SourceProvider.Interfaces;
using Utilities.ORM.Parameters;

namespace Utilities.ORM.Manager
{
    /// <summary>
    /// Session object
    /// </summary>
    public class Session
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Session()
        {
            QueryProvider = IoC.Manager.Bootstrapper.Resolve<QueryProvider.Manager>();
            SourceProvider = IoC.Manager.Bootstrapper.Resolve<SourceProvider.Manager>();
            MapperProvider = IoC.Manager.Bootstrapper.Resolve<Mapper.Manager>();
            Cache = IoC.Manager.Bootstrapper.Resolve<DataTypes.Caching.Manager>().Cache();
        }

        /// <summary>
        /// Cache that is used
        /// </summary>
        private ICache Cache { get; set; }

        /// <summary>
        /// Mapper provider
        /// </summary>
        private Mapper.Manager MapperProvider { get; set; }

        /// <summary>
        /// Query provider
        /// </summary>
        private QueryProvider.Manager QueryProvider { get; set; }

        /// <summary>
        /// Source provider
        /// </summary>
        private SourceProvider.Manager SourceProvider { get; set; }

        /// <summary>
        /// Returns all items that match the criteria
        /// </summary>
        /// <typeparam name="ObjectType">Type of the object</typeparam>
        /// <param name="Parameters">Parameters used in the where clause</param>
        /// <returns>All items that match the criteria</returns>
        public IEnumerable<ObjectType> All<ObjectType>(params IParameter[] Parameters)
            where ObjectType : class,new()
        {
            Parameters = Parameters.Check(new IParameter[] { });
            List<Dynamo> ReturnValue = new List<Dynamo>();
            string KeyName = typeof(ObjectType).GetName() + "_All_" + Parameters.ToString(x => x.ToString(), "_");
            Parameters.ForEach(x => { KeyName = x.AddParameter(KeyName); });
            if (Cache.ContainsKey(KeyName))
            {
                ReturnValue = (List<Dynamo>)Cache[KeyName];
                return ReturnValue.ForEachParallel(x =>
                {
                    ObjectType Value = x.To<ObjectType>();
                    ((IORMObject)Value).Session0 = this;
                    return Value;
                });
            }
            foreach (ISourceInfo Source in SourceProvider.Where(x => x.Readable).OrderBy(x => x.Order))
            {
                IMapping Mapping = MapperProvider[typeof(ObjectType), Source];
                if (Mapping != null)
                {
                    foreach (Dynamo Item in QueryProvider.Generate<ObjectType>(Source, Mapping).All(Parameters).Execute()[0])
                    {
                        IProperty IDProperty = Mapping.IDProperties.FirstOrDefault();
                        object IDValue = IDProperty.GetValue(Item);
                        Dynamo Value = ReturnValue.FirstOrDefault(x => IDProperty.GetValue(x).Equals(IDValue));
                        if (Value == null)
                            ReturnValue.Add(Item);
                        else
                            Item.CopyTo(Value);
                    }
                }
            }
            Cache.Add(KeyName, ReturnValue, new string[] { typeof(ObjectType).GetName() });
            return ReturnValue.ForEachParallel(x =>
            {
                ObjectType Value = x.To<ObjectType>();
                ((IORMObject)Value).Session0 = this;
                return Value;
            });
        }

        /// <summary>
        /// Returns a single item matching the criteria
        /// </summary>
        /// <typeparam name="ObjectType">Type of the object</typeparam>
        /// <param name="Parameters">Parameters used in the where clause</param>
        /// <returns>A single object matching the criteria</returns>
        public ObjectType Any<ObjectType>(params IParameter[] Parameters)
            where ObjectType : class,new()
        {
            Parameters = Parameters.Check(new IParameter[] { });
            Dynamo ReturnValue = null;
            string KeyName = typeof(ObjectType).GetName() + "_Any_" + Parameters.ToString(x => x.ToString(), "_");
            Parameters.ForEach(x => { KeyName = x.AddParameter(KeyName); });
            if (Cache.ContainsKey(KeyName))
            {
                ReturnValue = (Dynamo)Cache[KeyName];
                if (ReturnValue == null)
                    return default(ObjectType);
                return ReturnValue.To<ObjectType>().Chain(x => { ((IORMObject)x).Session0 = this; });
            }
            foreach (ISourceInfo Source in SourceProvider.Where(x => x.Readable).OrderBy(x => x.Order))
            {
                IMapping Mapping = MapperProvider[typeof(ObjectType), Source];
                if (Mapping != null)
                {
                    Dynamo Value = QueryProvider.Generate<ObjectType>(Source, Mapping).Any(Parameters).Execute()[0].FirstOrDefault();
                    if (Value != null)
                    {
                        if (ReturnValue == null)
                            ReturnValue = Value;
                        else
                            Value.CopyTo(ReturnValue);
                    }
                }
            }
            Cache.Add(KeyName, ReturnValue, new string[] { typeof(ObjectType).GetName() });
            if (ReturnValue == null)
                return default(ObjectType);
            return ReturnValue.To<ObjectType>().Chain(x => { ((IORMObject)x).Session0 = this; });
        }

        /// <summary>
        /// Returns a single item matching the criteria specified
        /// </summary>
        /// <typeparam name="ObjectType">Type of the object</typeparam>
        /// <typeparam name="IDType">ID type for the object</typeparam>
        /// <param name="ID">ID of the object to load</param>
        /// <returns>A single object matching the ID</returns>
        public ObjectType Any<ObjectType, IDType>(IDType ID)
            where ObjectType : class,new()
            where IDType : IComparable
        {
            Dynamo ReturnValue = null;
            string KeyName = typeof(ObjectType).GetName() + "_Any_" + ID.ToString();
            if (Cache.ContainsKey(KeyName))
            {
                ReturnValue = (Dynamo)Cache[KeyName];
                if (ReturnValue == null)
                    return default(ObjectType);
                return ReturnValue.To<ObjectType>().Chain(x => { ((IORMObject)x).Session0 = this; });
            }
            string StringID = ID.ToString();
            foreach (ISourceInfo Source in SourceProvider.Where(x => x.Readable).OrderBy(x => x.Order))
            {
                IMapping Mapping = MapperProvider[typeof(ObjectType), Source];
                if (Mapping != null)
                {
                    IProperty IDProperty = Mapping.IDProperties.FirstOrDefault();
                    if (IDProperty != null)
                    {
                        Dynamo Value = typeof(IDType) == typeof(string) ?
                            QueryProvider.Generate<ObjectType>(Source, Mapping).Any(new StringEqualParameter(StringID, IDProperty.FieldName, StringID.Length, IDProperty.FieldName, Source.ParameterPrefix)).Execute()[0].FirstOrDefault() :
                            QueryProvider.Generate<ObjectType>(Source, Mapping).Any(new EqualParameter<IDType>(ID, IDProperty.FieldName, IDProperty.FieldName, Source.ParameterPrefix)).Execute()[0].FirstOrDefault();
                        if (Value != null)
                        {
                            if (ReturnValue == null)
                                ReturnValue = Value;
                            else
                                Value.CopyTo(ReturnValue);
                        }
                    }
                }
            }
            Cache.Add(KeyName, ReturnValue, new string[] { typeof(ObjectType).GetName() });
            if (ReturnValue == null)
                return default(ObjectType);
            return ReturnValue.To<ObjectType>().Chain(x => { ((IORMObject)x).Session0 = this; });
        }

        /// <summary>
        /// Deletes an object from the database
        /// </summary>
        /// <typeparam name="ObjectType">Object type</typeparam>
        /// <param name="Object">Object to delete</param>
        public void Delete<ObjectType>(ObjectType Object)
            where ObjectType : class,new()
        {
            Cache.RemoveByTag(typeof(ObjectType).GetName());
            foreach (ISourceInfo Source in SourceProvider.Where(x => x.Writable).OrderBy(x => x.Order))
            {
                IMapping Mapping = MapperProvider[typeof(ObjectType), Source];
                if (Mapping != null)
                {
                    IGenerator<ObjectType> Generator = QueryProvider.Generate<ObjectType>(Source, MapperProvider[typeof(ObjectType), Source]);
                    IBatch TempBatch = QueryProvider.Batch(Source);
                    CascadeDelete<ObjectType>(Object, Source, Mapping, TempBatch, new List<object>());
                    TempBatch.AddCommand(Generator.Delete(Object));
                    TempBatch.Execute();
                }
            }
        }

        /// <summary>
        /// Loads a property (primarily used internally for lazy loading)
        /// </summary>
        /// <typeparam name="ObjectType">Object type</typeparam>
        /// <typeparam name="DataType">Data type</typeparam>
        /// <param name="Object">Object</param>
        /// <param name="PropertyName">Property name</param>
        /// <returns>The appropriate property value</returns>
        public List<DataType> LoadProperties<ObjectType, DataType>(ObjectType Object, string PropertyName)
            where ObjectType : class,new()
            where DataType : class,new()
        {
            System.Collections.Generic.List<Dynamo> ReturnValue = new System.Collections.Generic.List<Dynamo>();
            foreach (ISourceInfo Source in SourceProvider.Where(x => x.Readable).OrderBy(x => x.Order))
            {
                IMapping Mapping = MapperProvider[typeof(ObjectType), Source];
                if (Mapping != null)
                {
                    IProperty Property = Mapping.Properties.FirstOrDefault(x => x.Name == PropertyName);
                    if (Property != null)
                    {
                        foreach (Dynamo Item in QueryProvider.Generate<ObjectType>(Source, Mapping)
                            .LoadProperty<DataType>(Object, Property)
                            .Execute()[0])
                        {
                            IProperty IDProperty = Property.ForeignMapping.IDProperties.FirstOrDefault();
                            object IDValue = IDProperty.GetValue(Item);
                            Dynamo Value = ReturnValue.FirstOrDefault(x => IDProperty.GetValue(x).Equals(IDValue));
                            if (Value == null)
                                ReturnValue.Add(Item);
                            else
                            {
                                Item.CopyTo(Value);
                            }
                        }
                    }
                }
            }
            if (ReturnValue.Count == 0)
                return new List<DataType>();
            foreach (ISourceInfo Source in SourceProvider.Where(x => x.Readable).OrderBy(x => x.Order))
            {
                IMapping ObjectMapping = MapperProvider[typeof(ObjectType), Source];
                IMapping Mapping = MapperProvider[typeof(DataType), Source];
                if (Mapping != null)
                {
                    IProperty ObjectProperty = ObjectMapping == null ? null : ObjectMapping.Properties.FirstOrDefault(x => x.Name == PropertyName);
                    if (ObjectProperty == null)
                    {
                        IProperty IDProperty = Mapping.IDProperties.FirstOrDefault();
                        IParameter Parameter = null;
                        int Counter = 0;
                        foreach (Dynamo Item in ReturnValue)
                        {
                            if (IDProperty.GetParameter(Item) != null)
                            {
                                if (Parameter == null)
                                    Parameter = new EqualParameter<object>(IDProperty.GetParameter(Item), Counter.ToString(CultureInfo.InvariantCulture), IDProperty.FieldName, Source.ParameterPrefix);
                                else
                                    Parameter = new OrParameter(Parameter, new EqualParameter<object>(IDProperty.GetParameter(Item), Counter.ToString(CultureInfo.InvariantCulture), IDProperty.FieldName, Source.ParameterPrefix));
                                ++Counter;
                            }
                        }
                        if (Parameter != null)
                        {
                            foreach (Dynamo Item in QueryProvider.Generate<DataType>(Source, Mapping).All(Parameter).Execute()[0])
                            {
                                object IDValue = IDProperty.GetValue(Item);
                                Dynamo Value = ReturnValue.FirstOrDefault(x => IDProperty.GetValue(x).Equals(IDValue));
                                Item.CopyTo(Value);
                            }
                        }
                    }
                }
            }
            return ReturnValue.ForEachParallel(x =>
            {
                DataType Value = x.To<DataType>();
                ((IORMObject)Value).Session0 = this;
                return Value;
            }).ToList();
        }

        /// <summary>
        /// Loads a property (primarily used internally for lazy loading)
        /// </summary>
        /// <typeparam name="ObjectType">Object type</typeparam>
        /// <typeparam name="DataType">Data type</typeparam>
        /// <param name="Object">Object</param>
        /// <param name="PropertyName">Property name</param>
        /// <returns>The appropriate property value</returns>
        public DataType LoadProperty<ObjectType, DataType>(ObjectType Object, string PropertyName)
            where ObjectType : class,new()
            where DataType : class,new()
        {
            return LoadProperties<ObjectType, DataType>(Object, PropertyName).FirstOrDefault();
        }

        /// <summary>
        /// Gets the number of pages based on the specified
        /// </summary>
        /// <param name="PageSize">Page size</param>
        /// <param name="Parameters">Parameters to search by</param>
        /// <typeparam name="ObjectType">Object type to get the page count of</typeparam>
        /// <returns>The number of pages that the table contains for the specified page size</returns>
        public int PageCount<ObjectType>(int PageSize = 25, params IParameter[] Parameters)
            where ObjectType : class,new()
        {
            Parameters = Parameters.Check(new IParameter[] { });
            string KeyName = typeof(ObjectType).GetName() + "_PageCount_" + PageSize.ToString(CultureInfo.InvariantCulture) + "_" + Parameters.ToString(x => x.ToString(), "_");
            Parameters.ForEach(x => { KeyName = x.AddParameter(KeyName); });
            if (Cache.ContainsKey(KeyName))
            {
                return (int)Cache[KeyName];
            }
            foreach (ISourceInfo Source in SourceProvider.Where(x => x.Readable).OrderBy(x => x.Order))
            {
                IMapping Mapping = MapperProvider[typeof(ObjectType), Source];
                if (Mapping != null)
                {
                    int Count = QueryProvider.Generate<ObjectType>(Source, Mapping)
                        .PageCount(PageSize, Parameters)
                        .Execute()[0]
                        .FirstOrDefault()
                        .Total;
                    if (Count > 0)
                    {
                        int ReturnValue = (Count / PageSize) + (Count % PageSize > 0 ? 1 : 0);
                        Cache.Add(KeyName, ReturnValue, new string[] { typeof(ObjectType).GetName() });
                        return ReturnValue;
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// Returns a paged list of items
        /// </summary>
        /// <typeparam name="ObjectType">Object type</typeparam>
        /// <param name="PageSize">Page size</param>
        /// <param name="CurrentPage">Current page (starting with 0)</param>
        /// <param name="Parameters">Parameters used in the where clause</param>
        /// <returns>A paged list of items that match the criteria</returns>
        public IEnumerable<ObjectType> Paged<ObjectType>(int PageSize = 25, int CurrentPage = 0, params IParameter[] Parameters)
            where ObjectType : class,new()
        {
            Parameters = Parameters.Check(new IParameter[] { });
            string KeyName = typeof(ObjectType).GetName() + "_Paged_" + PageSize.ToString(CultureInfo.InvariantCulture) + "_" + CurrentPage.ToString(CultureInfo.InvariantCulture) + "_" + Parameters.ToString(x => x.ToString(), "_");
            Parameters.ForEach(x => { KeyName = x.AddParameter(KeyName); });
            System.Collections.Generic.List<Dynamo> ReturnValue = new System.Collections.Generic.List<Dynamo>();
            if (Cache.ContainsKey(KeyName))
            {
                ReturnValue = (List<Dynamo>)Cache[KeyName];
                return ReturnValue.ForEachParallel(x =>
                {
                    ObjectType Value = x.To<ObjectType>();
                    ((IORMObject)Value).Session0 = this;
                    return Value;
                });
            }
            foreach (ISourceInfo Source in SourceProvider.Where(x => x.Readable).OrderBy(x => x.Order))
            {
                IMapping Mapping = MapperProvider[typeof(ObjectType), Source];
                if (Mapping != null)
                {
                    IProperty IDProperty = Mapping.IDProperties.FirstOrDefault();
                    if (IDProperty != null)
                    {
                        foreach (Dynamo Item in QueryProvider.Generate<ObjectType>(Source, Mapping)
                            .Paged(PageSize, CurrentPage, Parameters)
                            .Execute()[0])
                        {
                            object IDValue = IDProperty.GetValue(Item);
                            Dynamo Value = ReturnValue.FirstOrDefault(x => IDProperty.GetValue(x).Equals(IDValue));
                            if (Value == null)
                                ReturnValue.Add(Item);
                            else
                                Item.CopyTo(Value);
                        }
                    }
                }
            }
            Cache.Add(KeyName, ReturnValue, new string[] { typeof(ObjectType).GetName() });
            return ReturnValue.ForEachParallel(x =>
            {
                ObjectType Value = x.To<ObjectType>();
                ((IORMObject)Value).Session0 = this;
                return Value;
            });
        }

        /// <summary>
        /// Saves an object to the database
        /// </summary>
        /// <typeparam name="ObjectType">Object type</typeparam>
        /// <typeparam name="PrimaryKeyType">Primary key type</typeparam>
        /// <param name="Object">Object to save</param>
        public void Save<ObjectType, PrimaryKeyType>(ObjectType Object)
            where ObjectType : class,new()
        {
            Cache.RemoveByTag(typeof(ObjectType).GetName());
            foreach (ISourceInfo Source in SourceProvider.Where(x => x.Writable).OrderBy(x => x.Order))
            {
                IMapping Mapping = MapperProvider[typeof(ObjectType), Source];
                if (Mapping != null)
                {
                    IGenerator<ObjectType> Generator = QueryProvider.Generate<ObjectType>(Source, MapperProvider[typeof(ObjectType), Source]);
                    IBatch TempBatch = QueryProvider.Batch(Source);
                    CascadeSave<ObjectType>(Object, Source, Mapping, TempBatch, new List<object>());
                    TempBatch.Execute();
                    TempBatch = QueryProvider.Batch(Source);
                    TempBatch.AddCommand(Generator.Save<PrimaryKeyType>(Object));
                    TempBatch.Execute();
                    TempBatch = QueryProvider.Batch(Source);
                    JoinsDelete<ObjectType>(Object, Source, Mapping, TempBatch, new List<object>());
                    JoinsSave<ObjectType>(Object, Source, Mapping, TempBatch, new List<object>());
                    TempBatch.RemoveDuplicateCommands().Execute();
                }
            }
        }

        private static void CascadeDelete<ObjectType>(ObjectType Object, ISourceInfo Source, IMapping Mapping, IBatch TempBatch, List<object> ObjectsSeen)
            where ObjectType : class, new()
        {
            Contract.Requires<ArgumentNullException>(Mapping != null, "Mapping");
            Contract.Requires<ArgumentNullException>(Mapping.Properties != null, "Mapping.Properties");
            foreach (IProperty<ObjectType> Property in Mapping.Properties.Where(x => x.Cascade))
            {
                TempBatch.AddCommand(Property.CascadeDelete(Object, Source, ObjectsSeen.ToList()));
            }
        }

        private static void CascadeSave<ObjectType>(ObjectType Object, ISourceInfo Source, IMapping Mapping, IBatch TempBatch, List<object> ObjectsSeen)
            where ObjectType : class, new()
        {
            Contract.Requires<ArgumentNullException>(Mapping != null, "Mapping");
            Contract.Requires<ArgumentNullException>(Mapping.Properties != null, "Mapping.Properties");
            foreach (IProperty<ObjectType> Property in Mapping.Properties.Where(x => x.Cascade))
            {
                TempBatch.AddCommand(Property.CascadeSave(Object, Source, ObjectsSeen.ToList()));
            }
        }

        private static void JoinsDelete<ObjectType>(ObjectType Object, ISourceInfo Source, IMapping Mapping, IBatch TempBatch, List<object> ObjectsSeen)
            where ObjectType : class, new()
        {
            Contract.Requires<ArgumentNullException>(Mapping != null, "Mapping");
            Contract.Requires<ArgumentNullException>(Mapping.Properties != null, "Mapping.Properties");
            foreach (IProperty<ObjectType> Property in Mapping.Properties)
            {
                if (!Property.Cascade &&
                    (Property is IManyToMany
                        || Property is IManyToOne
                        || Property is IIEnumerableManyToOne
                        || Property is IListManyToMany
                        || Property is IListManyToOne))
                {
                    TempBatch.AddCommand(Property.JoinsDelete(Object, Source, ObjectsSeen.ToList()));
                }
                else if (Property.Cascade)
                {
                    TempBatch.AddCommand(Property.CascadeJoinsDelete(Object, Source, ObjectsSeen.ToList()));
                }
            }
        }

        private static void JoinsSave<ObjectType>(ObjectType Object, ISourceInfo Source, IMapping Mapping, IBatch TempBatch, List<object> ObjectsSeen)
            where ObjectType : class, new()
        {
            Contract.Requires<ArgumentNullException>(Mapping != null, "Mapping");
            Contract.Requires<ArgumentNullException>(Mapping.Properties != null, "Mapping.Properties");
            foreach (IProperty<ObjectType> Property in Mapping.Properties)
            {
                if (!Property.Cascade &&
                    (Property is IManyToMany
                        || Property is IManyToOne
                        || Property is IIEnumerableManyToOne
                        || Property is IListManyToMany
                        || Property is IListManyToOne))
                {
                    TempBatch.AddCommand(Property.JoinsSave(Object, Source, ObjectsSeen.ToList()));
                }
                else if (Property.Cascade)
                {
                    TempBatch.AddCommand(Property.CascadeJoinsSave(Object, Source, ObjectsSeen.ToList()));
                }
            }
        }
    }
}