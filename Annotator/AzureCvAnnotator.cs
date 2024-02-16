using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Linq;

namespace AzureComputerVision {
    public class AzureCvAnnotator {
        public AzureCvAnnotator() { }
        public MpAnalyzerPluginResponseFormat CreateAnnotations(MpAnalyzerPluginResponseFormat resp, string respJsonStr) {
            Root root = respJsonStr.DeserializeObject<Root>();
            List<MpAnnotationNodeFormat> annotations = new List<MpAnnotationNodeFormat>();

            if (ProcessCategories(root.categories) is MpAnnotationNodeFormat cat_an) {
                annotations.Add(cat_an);
            }
            if (ProcessTags(root.tags) is MpAnnotationNodeFormat tags_an) {
                annotations.Add(tags_an);
            }
            if (ProcessObjects(root.objects) is MpAnnotationNodeFormat obj_an) {
                annotations.Add(obj_an);
            }
            if (ProcessAdult(root.adult) is MpAnnotationNodeFormat ad_an) {
                annotations.Add(ad_an);
            }
            if (ProcessDescription(root.description) is MpAnnotationNodeFormat description_anf) {
                annotations.Add(description_anf);
            }
            if (ProcessMetaData(root.metadata) is MpAnnotationNodeFormat metadata_anf) {
                annotations.Add(metadata_anf);
            }
            if (ProcessFaces(root.faces) is MpAnnotationNodeFormat faces_an) {
                annotations.Add(faces_an);
            }
            if (ProcessColor(root.color) is MpAnnotationNodeFormat color_an) {
                annotations.Add(color_an);
            }
            if (ProcessImageType(root.imageType) is MpAnnotationNodeFormat it_an) {
                annotations.Add(it_an);
            }
            if (ProcessRequestInfo(root) is MpAnnotationNodeFormat ri_an) {
                annotations.Add(ri_an);
            }

            if (annotations.Count > 0) {
                var root_annotation = new MpAnnotationNodeFormat() {
                    label = "Azure Image Analysis",
                    children = annotations
                };
                resp.dataObjectLookup = new Dictionary<string, object>() {
                    {
                        MpPortableDataFormats.INTERNAL_CONTENT_ANNOTATION_FORMAT,
                        root_annotation.SerializeObject(true)
                    } };
            }



            return resp;
        }

        MpAnnotationNodeFormat ProcessObjects(List<VisionObject> vobjects) {
            if (vobjects == null || vobjects.Count == 0) {
                return null;
            }
            return new MpAnnotationNodeFormat() {
                type = "Objects",
                label = "Objects",
                children = vobjects.Select(x => new MpImageAnnotationNodeFormat() {
                    type = "Object",
                    label = x.@object,
                    score = x.confidence,
                    left = x.rectangle.x,
                    top = x.rectangle.y,
                    right = x.rectangle.x + x.rectangle.w,
                    bottom = x.rectangle.y + x.rectangle.h

                    //children = ProcessRectangle(x.rectangle, "Object") is MpAnnotationNodeFormat da ? new List<MpAnnotationNodeFormat>() { da } : null
                }).Cast<MpAnnotationNodeFormat>().ToList()
            };
        }

        MpAnnotationNodeFormat ProcessTags(List<Tag> tags) {
            if (tags == null || tags.Count == 0) {
                return null;
            }
            return new MpAnnotationNodeFormat() {
                type = "Tags",
                label = "Tags",
                children = tags.Select(x => new MpAnnotationNodeFormat() {
                    type = "Tag",
                    label = x.name,
                    score = x.confidence
                }).ToList()
            };
        }
        MpAnnotationNodeFormat ProcessCategories(List<Category> categories) {
            if (categories == null || categories.Count == 0) {
                return null;
            }
            return new MpAnnotationNodeFormat() {
                type = "Categories",
                label = "Categories",
                children = categories.Select(x => new MpAnnotationNodeFormat() {
                    type = "Category",
                    label = x.name,
                    score = x.score,
                    children = ProcessDetail(x.detail)
                }).ToList()
            };
        }
        List<MpAnnotationNodeFormat> ProcessDetail(Detail detail) {
            if (detail == null) {
                return null;
            }
            List<MpAnnotationNodeFormat> children = new List<MpAnnotationNodeFormat>();
            if (detail.celebrities != null &&
                detail.celebrities.Count > 0) {
                if (detail.celebrities.Select(x => ProcessCelebrity(x)) is IEnumerable<MpAnnotationNodeFormat> al &&
                    al.Where(x => x != null).Count() > 0) {
                    children.AddRange(al.Where(x => x != null));
                }
            }
            if (detail.landmarks != null &&
                detail.landmarks.Count > 0) {
                if (detail.landmarks.Select(x => ProcessLandmark(x)) is IEnumerable<MpAnnotationNodeFormat> al &&
                    al.Where(x => x != null).Count() > 0) {
                    children.AddRange(al.Where(x => x != null));
                }
            }
            if (children.Count == 0) {
                return null;
            }

            //return new MpAnnotationNodeFormat() {
            //    type = "Detail",
            //    children = children
            //};
            return children;
        }

        MpAnnotationNodeFormat ProcessCelebrity(Celebrity celeb) {
            if (celeb == null) {
                return null;
            }
            return new MpAnnotationNodeFormat() {
                type = "Celebrity",
                label = celeb.name,
                score = celeb.confidence,
                children = celeb.faceRectangle == null ? null : new List<MpAnnotationNodeFormat>() { ProcessFaceRectangle(celeb.faceRectangle) }
            };
        }

        MpAnnotationNodeFormat ProcessLandmark(Landmark landmark) {
            if (landmark == null) {
                return null;
            }
            return new MpAnnotationNodeFormat() {
                type = "Landmark",
                label = landmark.name,
                score = landmark.confidence
            };
        }

        MpAnnotationNodeFormat ProcessFaceRectangle(FaceRectangle fr) {
            if (fr == null) {
                return null;
            }
            return new MpImageAnnotationNodeFormat() {
                type = "FaceRectangle",
                left = fr.left,
                top = fr.top,
                right = fr.left + fr.width,
                bottom = fr.top + fr.height
            };
        }

        MpAnnotationNodeFormat ProcessRectangle(Rectangle rect, string rectType) {
            if (rect == null) {
                return null;
            }
            return new MpImageAnnotationNodeFormat() {
                type = rectType,
                left = rect.x,
                top = rect.y,
                right = rect.x + rect.w,
                bottom = rect.y + rect.h
            };
        }

        MpAnnotationNodeFormat ProcessAdult(Adult adult) {
            if (adult == null) {
                return null;
            }
            return new MpAnnotationNodeFormat() {
                type = "Adult",
                label = "Adult Classifications",
                children = new List<MpAnnotationNodeFormat>() {
                    new MpAnnotationNodeFormat() {
                        type = "Adult",
                        label = "Adult Content?",
                        body = adult.isAdultContent.ToString(),
                        score = adult.adultScore
                    },
                    new MpAnnotationNodeFormat() {
                        type = "Adult",
                        label = "Racy Content?",
                        body = adult.isRacyContent.ToString(),
                        score = adult.racyScore
                    },
                    new MpAnnotationNodeFormat() {
                        type = "Adult",
                        label = "Gory Content?",
                        body = adult.isGoryContent.ToString(),
                        score = adult.goreScore
                    }
                }
            };
        }

        MpAnnotationNodeFormat ProcessDescription(Description d) {
            if (d == null) {
                return null;
            }
            var danf = new MpAnnotationNodeFormat() {
                type = "Description",
                label = "Descriptions",
                children = new List<MpAnnotationNodeFormat>()
            };
            if (d.tags != null && d.tags.Count > 0) {
                danf.children.Add(
                    new MpAnnotationNodeFormat() {
                        type = "Tags",
                        label = "Tags",
                        children =
                            d.tags.Select(x => new MpAnnotationNodeFormat() {
                                type = "Tag",
                                label = x
                            }).ToList()
                    });
            }
            if (d.captions != null && d.captions.Count > 0) {
                danf.children.Add(
                    new MpAnnotationNodeFormat() {
                        type = "Captions",
                        label = "Captions",
                        children =
                        d.captions.Select(x => new MpAnnotationNodeFormat() {
                            type = "Caption",
                            label = x.text,
                            score = x.confidence
                        }).ToList()
                    });
            }
            if (danf.children.Count == 0) {
                return null;
            }
            return danf;
        }

        MpAnnotationNodeFormat ProcessMetaData(Metadata md) {
            if (md == null) {
                return null;
            }
            return new MpAnnotationNodeFormat() {
                type = "MetaData",
                label = "MetaData",
                children = new List<MpAnnotationNodeFormat>() {
                    new MpAnnotationNodeFormat() {
                        type = "ImageDimension",
                        label = "Width",
                        body = md.width.ToString()
                    },
                    new MpAnnotationNodeFormat() {
                        type = "ImageDimension",
                        label = "Height",
                        body = md.height.ToString()
                    },
                    new MpAnnotationNodeFormat() {
                        type = "ImageFormat",
                        label = "Format",
                        body = md.format
                    },
                }
            };
        }

        MpAnnotationNodeFormat ProcessFaces(List<Face> faces) {
            if (faces == null || faces.Count == 0) {
                return null;
            }
            return new MpAnnotationNodeFormat() {
                type = "Faces",
                label = "Faces",
                children = faces.Select(x => new MpAnnotationNodeFormat() {
                    type = "Face",
                    label = "Faces",
                    children = new List<MpAnnotationNodeFormat>() {
                        new MpAnnotationNodeFormat() {
                            type = "Age",
                            label = "Age",
                            body = x.age.ToString()
                        },
                        new MpAnnotationNodeFormat() {
                            type = "Gender",
                            label = "Gender",
                            body = x.gender
                        },
                        ProcessFaceRectangle(x.faceRectangle)
                    }.ToList()
                }).ToList()
            };
        }

        MpAnnotationNodeFormat ProcessColor(Color color) {
            if (color == null) {
                return null;
            }
            return new MpAnnotationNodeFormat() {
                type = "Colors",
                label = "Colors",
                children = new[] {
                    new MpAnnotationNodeFormat() {
                        type = "Color",
                        label = "Dominant Foreground Color",
                        body = color.dominantColorForeground
                    },
                    new MpAnnotationNodeFormat() {
                        type = "Color",
                        label = "Dominant Background Color",
                        body = color.dominantColorBackground
                    },
                    new MpAnnotationNodeFormat() {
                        type = "Color",
                        label = "Accent Color",
                        body = color.accentColor
                    },
                    new MpAnnotationNodeFormat() {
                        type = "Color",
                        label = "Black and White?",
                        body = color.isBWImg.ToString()
                    }
                }.Union(color.dominantColors.Select(x => new MpAnnotationNodeFormat() {
                    type = "Color",
                    label = "Dominant Color",
                    body = x
                })).ToList()
            };
        }

        MpAnnotationNodeFormat ProcessImageType(ImageType it) {
            if (it == null) {
                return null;
            }
            return new MpAnnotationNodeFormat() {
                type = "ImageType",
                label = "Image Type",
                children = new List<MpAnnotationNodeFormat>() {
                    new MpAnnotationNodeFormat() {
                        type = "ImageType",
                        label = "Clip Art Type",
                        body = ((ImageClipArtType)it.clipArtType).ToString()
                    },
                    new MpAnnotationNodeFormat() {
                        type = "ImageType",
                        label = "Line Drawing?",
                        body = it.lineDrawingType > 0 ? "True":"False"
                    }
                }
            };
        }

        MpAnnotationNodeFormat ProcessRequestInfo(Root r) {
            if (r == null) {
                return null;
            }
            return new MpAnnotationNodeFormat() {
                type = "RequestInfo",
                label = "Azure Request Info",
                children = new List<MpAnnotationNodeFormat>() {
                    new MpAnnotationNodeFormat() {
                        type = "RequestId",
                        label = "RequestId",
                        body = r.requestId.ToString()
                    },
                    new MpAnnotationNodeFormat() {
                        type = "ModelVersion",
                        label = "Model Version",
                        body = r.modelVersion
                    }
                }
            };
        }
    }
}
