; ModuleID = 'compressed_assemblies.arm64-v8a.ll'
source_filename = "compressed_assemblies.arm64-v8a.ll"
target datalayout = "e-m:e-i8:8:32-i16:16:32-i64:64-i128:128-n32:64-S128"
target triple = "aarch64-unknown-linux-android21"

%struct.CompressedAssemblies = type {
	i32, ; uint32_t count
	ptr ; CompressedAssemblyDescriptor descriptors
}

%struct.CompressedAssemblyDescriptor = type {
	i32, ; uint32_t uncompressed_file_size
	i1, ; bool loaded
	ptr ; uint8_t data
}

@compressed_assemblies = dso_local local_unnamed_addr global %struct.CompressedAssemblies {
	i32 92, ; uint32_t count
	ptr @compressed_assembly_descriptors; CompressedAssemblyDescriptor* descriptors
}, align 8

@compressed_assembly_descriptors = internal dso_local global [92 x %struct.CompressedAssemblyDescriptor] [
	%struct.CompressedAssemblyDescriptor {
		i32 6144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_0; uint8_t* data
	}, ; 0: _Microsoft.Android.Resource.Designer
	%struct.CompressedAssemblyDescriptor {
		i32 14848, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_1; uint8_t* data
	}, ; 1: Microsoft.Extensions.Configuration
	%struct.CompressedAssemblyDescriptor {
		i32 6144, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_2; uint8_t* data
	}, ; 2: Microsoft.Extensions.Configuration.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 46080, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_3; uint8_t* data
	}, ; 3: Microsoft.Extensions.DependencyInjection
	%struct.CompressedAssemblyDescriptor {
		i32 26112, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_4; uint8_t* data
	}, ; 4: Microsoft.Extensions.DependencyInjection.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 17920, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_5; uint8_t* data
	}, ; 5: Microsoft.Extensions.Logging
	%struct.CompressedAssemblyDescriptor {
		i32 19456, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_6; uint8_t* data
	}, ; 6: Microsoft.Extensions.Logging.Abstractions
	%struct.CompressedAssemblyDescriptor {
		i32 17408, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_7; uint8_t* data
	}, ; 7: Microsoft.Extensions.Options
	%struct.CompressedAssemblyDescriptor {
		i32 9216, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_8; uint8_t* data
	}, ; 8: Microsoft.Extensions.Primitives
	%struct.CompressedAssemblyDescriptor {
		i32 1877816, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_9; uint8_t* data
	}, ; 9: Microsoft.Maui.Controls
	%struct.CompressedAssemblyDescriptor {
		i32 128264, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_10; uint8_t* data
	}, ; 10: Microsoft.Maui.Controls.Xaml
	%struct.CompressedAssemblyDescriptor {
		i32 802608, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_11; uint8_t* data
	}, ; 11: Microsoft.Maui
	%struct.CompressedAssemblyDescriptor {
		i32 81408, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_12; uint8_t* data
	}, ; 12: Microsoft.Maui.Essentials
	%struct.CompressedAssemblyDescriptor {
		i32 207640, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_13; uint8_t* data
	}, ; 13: Microsoft.Maui.Graphics
	%struct.CompressedAssemblyDescriptor {
		i32 107520, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_14; uint8_t* data
	}, ; 14: SQLite-net
	%struct.CompressedAssemblyDescriptor {
		i32 5632, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_15; uint8_t* data
	}, ; 15: SQLitePCLRaw.batteries_v2
	%struct.CompressedAssemblyDescriptor {
		i32 50688, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_16; uint8_t* data
	}, ; 16: SQLitePCLRaw.core
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_17; uint8_t* data
	}, ; 17: SQLitePCLRaw.lib.e_sqlite3.android
	%struct.CompressedAssemblyDescriptor {
		i32 35840, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_18; uint8_t* data
	}, ; 18: SQLitePCLRaw.provider.e_sqlite3
	%struct.CompressedAssemblyDescriptor {
		i32 59392, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_19; uint8_t* data
	}, ; 19: Xamarin.AndroidX.Activity
	%struct.CompressedAssemblyDescriptor {
		i32 515584, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_20; uint8_t* data
	}, ; 20: Xamarin.AndroidX.AppCompat
	%struct.CompressedAssemblyDescriptor {
		i32 15872, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_21; uint8_t* data
	}, ; 21: Xamarin.AndroidX.AppCompat.AppCompatResources
	%struct.CompressedAssemblyDescriptor {
		i32 16384, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_22; uint8_t* data
	}, ; 22: Xamarin.AndroidX.CardView
	%struct.CompressedAssemblyDescriptor {
		i32 19456, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_23; uint8_t* data
	}, ; 23: Xamarin.AndroidX.Collection.Jvm
	%struct.CompressedAssemblyDescriptor {
		i32 71680, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_24; uint8_t* data
	}, ; 24: Xamarin.AndroidX.CoordinatorLayout
	%struct.CompressedAssemblyDescriptor {
		i32 743936, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_25; uint8_t* data
	}, ; 25: Xamarin.AndroidX.Core
	%struct.CompressedAssemblyDescriptor {
		i32 24576, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_26; uint8_t* data
	}, ; 26: Xamarin.AndroidX.CursorAdapter
	%struct.CompressedAssemblyDescriptor {
		i32 9728, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_27; uint8_t* data
	}, ; 27: Xamarin.AndroidX.CustomView
	%struct.CompressedAssemblyDescriptor {
		i32 42496, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_28; uint8_t* data
	}, ; 28: Xamarin.AndroidX.DrawerLayout
	%struct.CompressedAssemblyDescriptor {
		i32 209920, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_29; uint8_t* data
	}, ; 29: Xamarin.AndroidX.Fragment
	%struct.CompressedAssemblyDescriptor {
		i32 21504, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_30; uint8_t* data
	}, ; 30: Xamarin.AndroidX.Lifecycle.Common.Jvm
	%struct.CompressedAssemblyDescriptor {
		i32 17408, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_31; uint8_t* data
	}, ; 31: Xamarin.AndroidX.Lifecycle.LiveData.Core
	%struct.CompressedAssemblyDescriptor {
		i32 32256, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_32; uint8_t* data
	}, ; 32: Xamarin.AndroidX.Lifecycle.ViewModel.Android
	%struct.CompressedAssemblyDescriptor {
		i32 12800, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_33; uint8_t* data
	}, ; 33: Xamarin.AndroidX.Lifecycle.ViewModelSavedState
	%struct.CompressedAssemblyDescriptor {
		i32 36352, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_34; uint8_t* data
	}, ; 34: Xamarin.AndroidX.Loader
	%struct.CompressedAssemblyDescriptor {
		i32 90112, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_35; uint8_t* data
	}, ; 35: Xamarin.AndroidX.Navigation.Common
	%struct.CompressedAssemblyDescriptor {
		i32 18432, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_36; uint8_t* data
	}, ; 36: Xamarin.AndroidX.Navigation.Fragment
	%struct.CompressedAssemblyDescriptor {
		i32 58880, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_37; uint8_t* data
	}, ; 37: Xamarin.AndroidX.Navigation.Runtime
	%struct.CompressedAssemblyDescriptor {
		i32 28160, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_38; uint8_t* data
	}, ; 38: Xamarin.AndroidX.Navigation.UI
	%struct.CompressedAssemblyDescriptor {
		i32 405504, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_39; uint8_t* data
	}, ; 39: Xamarin.AndroidX.RecyclerView
	%struct.CompressedAssemblyDescriptor {
		i32 11264, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_40; uint8_t* data
	}, ; 40: Xamarin.AndroidX.SavedState
	%struct.CompressedAssemblyDescriptor {
		i32 37888, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_41; uint8_t* data
	}, ; 41: Xamarin.AndroidX.SwipeRefreshLayout
	%struct.CompressedAssemblyDescriptor {
		i32 9216, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_42; uint8_t* data
	}, ; 42: Xamarin.AndroidX.VersionedParcelable
	%struct.CompressedAssemblyDescriptor {
		i32 57344, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_43; uint8_t* data
	}, ; 43: Xamarin.AndroidX.ViewPager
	%struct.CompressedAssemblyDescriptor {
		i32 38400, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_44; uint8_t* data
	}, ; 44: Xamarin.AndroidX.ViewPager2
	%struct.CompressedAssemblyDescriptor {
		i32 583168, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_45; uint8_t* data
	}, ; 45: Xamarin.Google.Android.Material
	%struct.CompressedAssemblyDescriptor {
		i32 86016, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_46; uint8_t* data
	}, ; 46: Xamarin.Kotlin.StdLib
	%struct.CompressedAssemblyDescriptor {
		i32 18432, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_47; uint8_t* data
	}, ; 47: Xamarin.KotlinX.Coroutines.Core.Jvm
	%struct.CompressedAssemblyDescriptor {
		i32 89600, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_48; uint8_t* data
	}, ; 48: Xamarin.KotlinX.Serialization.Core.Jvm
	%struct.CompressedAssemblyDescriptor {
		i32 482816, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_49; uint8_t* data
	}, ; 49: DailyExpenseTracker
	%struct.CompressedAssemblyDescriptor {
		i32 24064, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_50; uint8_t* data
	}, ; 50: System.Collections.Concurrent
	%struct.CompressedAssemblyDescriptor {
		i32 14848, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_51; uint8_t* data
	}, ; 51: System.Collections.NonGeneric
	%struct.CompressedAssemblyDescriptor {
		i32 9216, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_52; uint8_t* data
	}, ; 52: System.Collections.Specialized
	%struct.CompressedAssemblyDescriptor {
		i32 34304, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_53; uint8_t* data
	}, ; 53: System.Collections
	%struct.CompressedAssemblyDescriptor {
		i32 10752, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_54; uint8_t* data
	}, ; 54: System.ComponentModel.Primitives
	%struct.CompressedAssemblyDescriptor {
		i32 15872, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_55; uint8_t* data
	}, ; 55: System.ComponentModel.TypeConverter
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_56; uint8_t* data
	}, ; 56: System.ComponentModel
	%struct.CompressedAssemblyDescriptor {
		i32 11776, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_57; uint8_t* data
	}, ; 57: System.Console
	%struct.CompressedAssemblyDescriptor {
		i32 18432, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_58; uint8_t* data
	}, ; 58: System.Diagnostics.DiagnosticSource
	%struct.CompressedAssemblyDescriptor {
		i32 59904, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_59; uint8_t* data
	}, ; 59: System.Formats.Asn1
	%struct.CompressedAssemblyDescriptor {
		i32 22016, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_60; uint8_t* data
	}, ; 60: System.IO.Compression.Brotli
	%struct.CompressedAssemblyDescriptor {
		i32 31232, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_61; uint8_t* data
	}, ; 61: System.IO.Compression
	%struct.CompressedAssemblyDescriptor {
		i32 27136, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_62; uint8_t* data
	}, ; 62: System.IO.Pipelines
	%struct.CompressedAssemblyDescriptor {
		i32 355840, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_63; uint8_t* data
	}, ; 63: System.Linq.Expressions
	%struct.CompressedAssemblyDescriptor {
		i32 50176, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_64; uint8_t* data
	}, ; 64: System.Linq
	%struct.CompressedAssemblyDescriptor {
		i32 16384, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_65; uint8_t* data
	}, ; 65: System.Memory
	%struct.CompressedAssemblyDescriptor {
		i32 129536, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_66; uint8_t* data
	}, ; 66: System.Net.Http
	%struct.CompressedAssemblyDescriptor {
		i32 37376, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_67; uint8_t* data
	}, ; 67: System.Net.Primitives
	%struct.CompressedAssemblyDescriptor {
		i32 7168, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_68; uint8_t* data
	}, ; 68: System.Net.Requests
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_69; uint8_t* data
	}, ; 69: System.Numerics.Vectors
	%struct.CompressedAssemblyDescriptor {
		i32 17920, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_70; uint8_t* data
	}, ; 70: System.ObjectModel
	%struct.CompressedAssemblyDescriptor {
		i32 69120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_71; uint8_t* data
	}, ; 71: System.Private.Uri
	%struct.CompressedAssemblyDescriptor {
		i32 382464, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_72; uint8_t* data
	}, ; 72: System.Private.Xml
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_73; uint8_t* data
	}, ; 73: System.Runtime.InteropServices.RuntimeInformation
	%struct.CompressedAssemblyDescriptor {
		i32 9728, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_74; uint8_t* data
	}, ; 74: System.Runtime.InteropServices
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_75; uint8_t* data
	}, ; 75: System.Runtime.Loader
	%struct.CompressedAssemblyDescriptor {
		i32 73728, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_76; uint8_t* data
	}, ; 76: System.Runtime.Numerics
	%struct.CompressedAssemblyDescriptor {
		i32 13824, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_77; uint8_t* data
	}, ; 77: System.Runtime
	%struct.CompressedAssemblyDescriptor {
		i32 121856, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_78; uint8_t* data
	}, ; 78: System.Security.Cryptography
	%struct.CompressedAssemblyDescriptor {
		i32 33280, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_79; uint8_t* data
	}, ; 79: System.Text.Encodings.Web
	%struct.CompressedAssemblyDescriptor {
		i32 365568, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_80; uint8_t* data
	}, ; 80: System.Text.Json
	%struct.CompressedAssemblyDescriptor {
		i32 311296, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_81; uint8_t* data
	}, ; 81: System.Text.RegularExpressions
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_82; uint8_t* data
	}, ; 82: System.Threading.Thread
	%struct.CompressedAssemblyDescriptor {
		i32 12288, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_83; uint8_t* data
	}, ; 83: System.Threading
	%struct.CompressedAssemblyDescriptor {
		i32 11264, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_84; uint8_t* data
	}, ; 84: System.Web.HttpUtility
	%struct.CompressedAssemblyDescriptor {
		i32 5120, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_85; uint8_t* data
	}, ; 85: System.Xml.ReaderWriter
	%struct.CompressedAssemblyDescriptor {
		i32 4608, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_86; uint8_t* data
	}, ; 86: System
	%struct.CompressedAssemblyDescriptor {
		i32 9216, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_87; uint8_t* data
	}, ; 87: netstandard
	%struct.CompressedAssemblyDescriptor {
		i32 1869824, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_88; uint8_t* data
	}, ; 88: System.Private.CoreLib
	%struct.CompressedAssemblyDescriptor {
		i32 166912, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_89; uint8_t* data
	}, ; 89: Java.Interop
	%struct.CompressedAssemblyDescriptor {
		i32 19512, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_90; uint8_t* data
	}, ; 90: Mono.Android.Runtime
	%struct.CompressedAssemblyDescriptor {
		i32 1898496, ; uint32_t uncompressed_file_size
		i1 false, ; bool loaded
		ptr @__compressedAssemblyData_91; uint8_t* data
	} ; 91: Mono.Android
], align 8

@__compressedAssemblyData_0 = internal dso_local global [6144 x i8] zeroinitializer, align 1
@__compressedAssemblyData_1 = internal dso_local global [14848 x i8] zeroinitializer, align 1
@__compressedAssemblyData_2 = internal dso_local global [6144 x i8] zeroinitializer, align 1
@__compressedAssemblyData_3 = internal dso_local global [46080 x i8] zeroinitializer, align 1
@__compressedAssemblyData_4 = internal dso_local global [26112 x i8] zeroinitializer, align 1
@__compressedAssemblyData_5 = internal dso_local global [17920 x i8] zeroinitializer, align 1
@__compressedAssemblyData_6 = internal dso_local global [19456 x i8] zeroinitializer, align 1
@__compressedAssemblyData_7 = internal dso_local global [17408 x i8] zeroinitializer, align 1
@__compressedAssemblyData_8 = internal dso_local global [9216 x i8] zeroinitializer, align 1
@__compressedAssemblyData_9 = internal dso_local global [1877816 x i8] zeroinitializer, align 1
@__compressedAssemblyData_10 = internal dso_local global [128264 x i8] zeroinitializer, align 1
@__compressedAssemblyData_11 = internal dso_local global [802608 x i8] zeroinitializer, align 1
@__compressedAssemblyData_12 = internal dso_local global [81408 x i8] zeroinitializer, align 1
@__compressedAssemblyData_13 = internal dso_local global [207640 x i8] zeroinitializer, align 1
@__compressedAssemblyData_14 = internal dso_local global [107520 x i8] zeroinitializer, align 1
@__compressedAssemblyData_15 = internal dso_local global [5632 x i8] zeroinitializer, align 1
@__compressedAssemblyData_16 = internal dso_local global [50688 x i8] zeroinitializer, align 1
@__compressedAssemblyData_17 = internal dso_local global [5120 x i8] zeroinitializer, align 1
@__compressedAssemblyData_18 = internal dso_local global [35840 x i8] zeroinitializer, align 1
@__compressedAssemblyData_19 = internal dso_local global [59392 x i8] zeroinitializer, align 1
@__compressedAssemblyData_20 = internal dso_local global [515584 x i8] zeroinitializer, align 1
@__compressedAssemblyData_21 = internal dso_local global [15872 x i8] zeroinitializer, align 1
@__compressedAssemblyData_22 = internal dso_local global [16384 x i8] zeroinitializer, align 1
@__compressedAssemblyData_23 = internal dso_local global [19456 x i8] zeroinitializer, align 1
@__compressedAssemblyData_24 = internal dso_local global [71680 x i8] zeroinitializer, align 1
@__compressedAssemblyData_25 = internal dso_local global [743936 x i8] zeroinitializer, align 1
@__compressedAssemblyData_26 = internal dso_local global [24576 x i8] zeroinitializer, align 1
@__compressedAssemblyData_27 = internal dso_local global [9728 x i8] zeroinitializer, align 1
@__compressedAssemblyData_28 = internal dso_local global [42496 x i8] zeroinitializer, align 1
@__compressedAssemblyData_29 = internal dso_local global [209920 x i8] zeroinitializer, align 1
@__compressedAssemblyData_30 = internal dso_local global [21504 x i8] zeroinitializer, align 1
@__compressedAssemblyData_31 = internal dso_local global [17408 x i8] zeroinitializer, align 1
@__compressedAssemblyData_32 = internal dso_local global [32256 x i8] zeroinitializer, align 1
@__compressedAssemblyData_33 = internal dso_local global [12800 x i8] zeroinitializer, align 1
@__compressedAssemblyData_34 = internal dso_local global [36352 x i8] zeroinitializer, align 1
@__compressedAssemblyData_35 = internal dso_local global [90112 x i8] zeroinitializer, align 1
@__compressedAssemblyData_36 = internal dso_local global [18432 x i8] zeroinitializer, align 1
@__compressedAssemblyData_37 = internal dso_local global [58880 x i8] zeroinitializer, align 1
@__compressedAssemblyData_38 = internal dso_local global [28160 x i8] zeroinitializer, align 1
@__compressedAssemblyData_39 = internal dso_local global [405504 x i8] zeroinitializer, align 1
@__compressedAssemblyData_40 = internal dso_local global [11264 x i8] zeroinitializer, align 1
@__compressedAssemblyData_41 = internal dso_local global [37888 x i8] zeroinitializer, align 1
@__compressedAssemblyData_42 = internal dso_local global [9216 x i8] zeroinitializer, align 1
@__compressedAssemblyData_43 = internal dso_local global [57344 x i8] zeroinitializer, align 1
@__compressedAssemblyData_44 = internal dso_local global [38400 x i8] zeroinitializer, align 1
@__compressedAssemblyData_45 = internal dso_local global [583168 x i8] zeroinitializer, align 1
@__compressedAssemblyData_46 = internal dso_local global [86016 x i8] zeroinitializer, align 1
@__compressedAssemblyData_47 = internal dso_local global [18432 x i8] zeroinitializer, align 1
@__compressedAssemblyData_48 = internal dso_local global [89600 x i8] zeroinitializer, align 1
@__compressedAssemblyData_49 = internal dso_local global [482816 x i8] zeroinitializer, align 1
@__compressedAssemblyData_50 = internal dso_local global [24064 x i8] zeroinitializer, align 1
@__compressedAssemblyData_51 = internal dso_local global [14848 x i8] zeroinitializer, align 1
@__compressedAssemblyData_52 = internal dso_local global [9216 x i8] zeroinitializer, align 1
@__compressedAssemblyData_53 = internal dso_local global [34304 x i8] zeroinitializer, align 1
@__compressedAssemblyData_54 = internal dso_local global [10752 x i8] zeroinitializer, align 1
@__compressedAssemblyData_55 = internal dso_local global [15872 x i8] zeroinitializer, align 1
@__compressedAssemblyData_56 = internal dso_local global [5120 x i8] zeroinitializer, align 1
@__compressedAssemblyData_57 = internal dso_local global [11776 x i8] zeroinitializer, align 1
@__compressedAssemblyData_58 = internal dso_local global [18432 x i8] zeroinitializer, align 1
@__compressedAssemblyData_59 = internal dso_local global [59904 x i8] zeroinitializer, align 1
@__compressedAssemblyData_60 = internal dso_local global [22016 x i8] zeroinitializer, align 1
@__compressedAssemblyData_61 = internal dso_local global [31232 x i8] zeroinitializer, align 1
@__compressedAssemblyData_62 = internal dso_local global [27136 x i8] zeroinitializer, align 1
@__compressedAssemblyData_63 = internal dso_local global [355840 x i8] zeroinitializer, align 1
@__compressedAssemblyData_64 = internal dso_local global [50176 x i8] zeroinitializer, align 1
@__compressedAssemblyData_65 = internal dso_local global [16384 x i8] zeroinitializer, align 1
@__compressedAssemblyData_66 = internal dso_local global [129536 x i8] zeroinitializer, align 1
@__compressedAssemblyData_67 = internal dso_local global [37376 x i8] zeroinitializer, align 1
@__compressedAssemblyData_68 = internal dso_local global [7168 x i8] zeroinitializer, align 1
@__compressedAssemblyData_69 = internal dso_local global [5120 x i8] zeroinitializer, align 1
@__compressedAssemblyData_70 = internal dso_local global [17920 x i8] zeroinitializer, align 1
@__compressedAssemblyData_71 = internal dso_local global [69120 x i8] zeroinitializer, align 1
@__compressedAssemblyData_72 = internal dso_local global [382464 x i8] zeroinitializer, align 1
@__compressedAssemblyData_73 = internal dso_local global [5120 x i8] zeroinitializer, align 1
@__compressedAssemblyData_74 = internal dso_local global [9728 x i8] zeroinitializer, align 1
@__compressedAssemblyData_75 = internal dso_local global [5120 x i8] zeroinitializer, align 1
@__compressedAssemblyData_76 = internal dso_local global [73728 x i8] zeroinitializer, align 1
@__compressedAssemblyData_77 = internal dso_local global [13824 x i8] zeroinitializer, align 1
@__compressedAssemblyData_78 = internal dso_local global [121856 x i8] zeroinitializer, align 1
@__compressedAssemblyData_79 = internal dso_local global [33280 x i8] zeroinitializer, align 1
@__compressedAssemblyData_80 = internal dso_local global [365568 x i8] zeroinitializer, align 1
@__compressedAssemblyData_81 = internal dso_local global [311296 x i8] zeroinitializer, align 1
@__compressedAssemblyData_82 = internal dso_local global [5120 x i8] zeroinitializer, align 1
@__compressedAssemblyData_83 = internal dso_local global [12288 x i8] zeroinitializer, align 1
@__compressedAssemblyData_84 = internal dso_local global [11264 x i8] zeroinitializer, align 1
@__compressedAssemblyData_85 = internal dso_local global [5120 x i8] zeroinitializer, align 1
@__compressedAssemblyData_86 = internal dso_local global [4608 x i8] zeroinitializer, align 1
@__compressedAssemblyData_87 = internal dso_local global [9216 x i8] zeroinitializer, align 1
@__compressedAssemblyData_88 = internal dso_local global [1869824 x i8] zeroinitializer, align 1
@__compressedAssemblyData_89 = internal dso_local global [166912 x i8] zeroinitializer, align 1
@__compressedAssemblyData_90 = internal dso_local global [19512 x i8] zeroinitializer, align 1
@__compressedAssemblyData_91 = internal dso_local global [1898496 x i8] zeroinitializer, align 1

; Metadata
!llvm.module.flags = !{!0, !1, !7, !8, !9, !10}
!0 = !{i32 1, !"wchar_size", i32 4}
!1 = !{i32 7, !"PIC Level", i32 2}
!llvm.ident = !{!2}
!2 = !{!".NET for Android remotes/origin/release/9.0.1xx @ 1dcfb6f8779c33b6f768c996495cb90ecd729329"}
!3 = !{!4, !4, i64 0}
!4 = !{!"any pointer", !5, i64 0}
!5 = !{!"omnipotent char", !6, i64 0}
!6 = !{!"Simple C++ TBAA"}
!7 = !{i32 1, !"branch-target-enforcement", i32 0}
!8 = !{i32 1, !"sign-return-address", i32 0}
!9 = !{i32 1, !"sign-return-address-all", i32 0}
!10 = !{i32 1, !"sign-return-address-with-bkey", i32 0}
