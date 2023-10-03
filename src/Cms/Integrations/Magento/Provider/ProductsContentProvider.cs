using System.Collections.Specialized;
using Cms.Integrations.Magento.Content;
using EPiServer.Construction;
using EPiServer.DataAccess;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Newtonsoft.Json;
using static Cms.Integrations.Magento.Content.CommerceContent;
using static Cms.Integrations.Magento.Content.ProductContent;

namespace Cms.Integrations.Magento.Provider;

public sealed class ProductsContentProvider : ContentProvider
    {
        private static readonly object Padlock = new object();
        private static ProductsContentProvider _instance = null;

        private readonly IdentityMappingService _identityMappingService;
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly IContentFactory _contentFactory;

        public const string DateFolderFormat = "dd-MM-yyyy";
        public const string Key = "externalproducts";

        private ProductsContentProvider()
        {
            _identityMappingService = ServiceLocator.Current.GetInstance<IdentityMappingService>();
            _contentTypeRepository = ServiceLocator.Current.GetInstance<IContentTypeRepository>();
            _contentFactory = ServiceLocator.Current.GetInstance<IContentFactory>();

            /* Create provider root if not exists
             * Provider root will be different for each site so that they can be cached and view seperately
             * Add configuration settings for entry point and capabilites */
            var socialRoot = GetEntryPoint();
            var providerValues = new NameValueCollection();
            providerValues.Add("entryPoint", socialRoot.ContentLink.ToString());
            providerValues.Add("capabilities", "Create, Edit, Delete, Search");

            /* initialize and register the provider */
            Initialize(Key, providerValues);
        }
        
        public static ProductsContentProvider Instance
        {
            get
            {
                lock (Padlock)
                {
                    if (_instance == null)
                    {
                        _instance = new ProductsContentProvider();
                    }

                    return _instance;
                }
            }
        }

        private static SiteDefinition CurrentDefinition => SiteDefinition.Current;

        private static IEnumerable<ProductItem> Products
        {
            get
            {
                using var r = new StreamReader("content.json");
                var json = r.ReadToEnd();
                var items = JsonConvert.DeserializeObject<List<ProductItem>>(json);

                return new List<ProductItem>(items);
            }
        }
        
        // TODO: LoadChildrenReferencesAndTypes
        
       // protected override void SetCacheSettings(ContentReference contentReference, IEnumerable<GetChildrenReferenceResult> children, CacheSettings cacheSettings)
       //  {
       //      // Set a low cache setting so new items are fetched from data source, but keep the
       //      // items already fetched for a long time in the cache.
       //      cacheSettings.SlidingExpiration = System.Web.Caching.Cache.NoSlidingExpiration;
       //      cacheSettings.AbsoluteExpiration = DateTime.Now.AddMinutes(5);
       //      
       //      base.SetCacheSettings(contentReference, children, cacheSettings);
       //  }

        protected override ContentResolveResult ResolveContent(ContentReference contentLink)
        {
            var mappedItem = _identityMappingService.Get(contentLink);
            if (mappedItem == null)
                return null;
            return ResolveContent(mappedItem);
        }

        protected override ContentResolveResult ResolveContent(Guid contentGuid)
        {
            var mappedItem = _identityMappingService.Get(contentGuid);
            if (mappedItem == null)
                return null;
            return ResolveContent(mappedItem);
        }

        private IContent LoadContent(MappedIdentity mappedIdentity, ILanguageSelector languageSelector)
        {
            CommerceType commerceType;
            SetContentType(mappedIdentity.ExternalIdentifier, out CommerceContentType commerceContentType, out commerceType);

            switch (commerceContentType)
            {
                case CommerceContentType.DateContentFolder:
                    return CreateFolder(mappedIdentity, commerceContentType, mappedIdentity.ExternalIdentifier.Segments[4]);
                case CommerceContentType.ContentFolder:
                    return CreateFolder(mappedIdentity, commerceContentType, commerceContentType.ToString());
                case CommerceContentType.Product:
                    {
                        var sku = mappedIdentity.ExternalIdentifier.Segments[4];
                        var product = Products.FirstOrDefault(p => p.Sku.Equals(sku));
                        return CreateProduct(mappedIdentity, commerceContentType, product);
                    }
            }

            return null;
        }

        protected override IContent LoadContent(ContentReference contentLink, ILanguageSelector languageSelector)
        {
            var mappedItem = _identityMappingService.Get(contentLink);
            return mappedItem == null ? null : LoadContent(mappedItem, languageSelector);
        }

        public static ContentFolder GetEntryPoint()
        {
            var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
            var folder = contentRepository.GetBySegment(ContentReference.RootPage, Key, LanguageSelector.AutoDetect()) as ContentFolder;
            if (folder == null)
            {
                folder = contentRepository.GetDefault<ContentFolder>(ContentReference.RootPage);
                folder.Name = Key;
                contentRepository.Save(folder, SaveAction.Publish, AccessLevel.NoAccess);
            }

            return folder;
        }
        

        private IEnumerable<GetChildrenReferenceResult> GetChildren<T>(List<T> inputList, Func<T, Uri> createUniqueUri, Type modelType, bool isLeafNode = false)
        {
           var children = new List<GetChildrenReferenceResult>();
           foreach (var t in inputList)
            {
                var externalIdentifier = _identityMappingService.Get(createUniqueUri(t), true);
                children.Add(new GetChildrenReferenceResult
                {
                    ContentLink = externalIdentifier.ContentLink,
                    IsLeafNode = isLeafNode,
                    ModelType = modelType
                });
            }

            return children;
        }

        private void SetContentType(Uri externalIdentifier, out CommerceContentType commerceContentType, out CommerceType commerceType)
        {
            /* Getting social type */
            commerceType = (CommerceType)Enum.Parse(typeof(CommerceType), RemoveEndingSlash(externalIdentifier.Segments[1]), true);

            /* Getting social content type */
            commerceContentType = (CommerceContentType)Enum.Parse(typeof(ContentType), RemoveEndingSlash(externalIdentifier.Segments[2]), true);
        }

        private IContent CreateContent(MappedIdentity mappedIdentity, CommerceContentType commerceContentType, Type modelType, string name, DateTime createDateTime)
        {
            return CreateContent(mappedIdentity.ContentLink.ID, mappedIdentity.ContentGuid, int.Parse(RemoveEndingSlash(mappedIdentity.ExternalIdentifier.Segments[3])), commerceContentType, modelType, name, createDateTime);
        }

        private IContent CreateContent(int contentId, Guid contentGuid, int parentContentId, CommerceContentType contentType, Type modelType, string name, DateTime createDateTime)
        {
            /* Find parent */
            var parentLink = EntryPoint;
            /* Getting parent id */
            if (contentType != CommerceContentType.ContentFolder)
                parentLink = new ContentReference(parentContentId, ProviderKey);

            EPiServer.DataAbstraction.ContentType epiContentType = _contentTypeRepository.Load(modelType);
            var content = _contentFactory.CreateContent(epiContentType);

            content.ContentTypeID = epiContentType.ID;
            content.ParentLink = parentLink;
            content.ContentGuid = contentGuid;
            content.ContentLink = new ContentReference(contentId, ProviderKey);
            content.Name = name;

            var securable = content as IContentSecurable;
            securable.GetContentSecurityDescriptor().AddEntry(new AccessControlEntry(EveryoneRole.RoleName, AccessLevel.Read));

            var versionable = content as IVersionable;
            if (versionable != null)
            {
                versionable.Status = VersionStatus.Published;
                versionable.IsPendingPublish = false;
                versionable.StartPublish = createDateTime.AddDays(-1);
            }

            var changeTrackable = content as IChangeTrackable;
            if (changeTrackable != null)
            {
                changeTrackable.Created = createDateTime;
                changeTrackable.Changed = createDateTime;
                changeTrackable.Saved = createDateTime;
            }

            return content;
        }

        private string RemoveEndingSlash(string virtualPath)
        {
            return !string.IsNullOrEmpty(virtualPath) && virtualPath[virtualPath.Length - 1] == '/' ? virtualPath.Substring(0, virtualPath.Length - 1) : virtualPath;
        }

        private CommerceContentFolder CreateFolder(MappedIdentity mappedIdentiy, CommerceContentType commerceType, string name)
        {
            var content = CreateContent(mappedIdentiy, commerceType, typeof(CommerceContentFolder), name, DateTime.Now) as CommerceContentFolder;
            return content;
        }

        private ProductContent CreateProduct(int contentId, Guid contentGuid, int parentContentId, CommerceContentType contentType, ProductItem productItem)
        {
            var name = productItem.Title;
            var content = CreateContent(contentId, contentGuid, parentContentId, contentType, typeof(ProductItem), name, DateTime.Now) as ProductContent;
            
            if (content == null) return default;
            
            content.Name = name;
            return content;

        }

        private ProductContent CreateProduct(MappedIdentity mappedIdentiy, CommerceContentType commerceContentType, ProductItem productItem)
        {
            return CreateProduct(mappedIdentiy.ContentLink.ID, mappedIdentiy.ContentGuid, int.Parse(RemoveEndingSlash(mappedIdentiy.ExternalIdentifier.Segments[3])), commerceContentType, productItem);
        }

        private ContentResolveResult ResolveContent(MappedIdentity mappedItem)
        {
            SetContentType(mappedItem.ExternalIdentifier, out var commerceContentType, out _);

            Type type = null;
            switch (commerceContentType)
            {
                case CommerceContentType.DateContentFolder:
                case CommerceContentType.ContentFolder:
                    type = typeof(CommerceContentFolder);
                    break;
                case CommerceContentType.Product:
                    type = typeof(ProductContent);
                    break;
            }

            var contentReference = new ContentReference(mappedItem.ContentLink.ID, ProviderKey);
            var contentType = _contentTypeRepository.Load(type);
            return new ContentResolveResult()
            {
                ContentLink = contentReference,
                UniqueID = mappedItem.ContentGuid,
                ContentUri = ConstructContentUri(contentType.ID, contentReference, mappedItem.ContentGuid)
            };
        }
    }
