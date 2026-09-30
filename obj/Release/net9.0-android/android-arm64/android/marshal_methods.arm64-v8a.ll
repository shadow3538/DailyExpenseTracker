; ModuleID = 'marshal_methods.arm64-v8a.ll'
source_filename = "marshal_methods.arm64-v8a.ll"
target datalayout = "e-m:e-i8:8:32-i16:16:32-i64:64-i128:128-n32:64-S128"
target triple = "aarch64-unknown-linux-android21"

%struct.MarshalMethodName = type {
	i64, ; uint64_t id
	ptr ; char* name
}

%struct.MarshalMethodsManagedClass = type {
	i32, ; uint32_t token
	ptr ; MonoClass klass
}

@assembly_image_cache = dso_local local_unnamed_addr global [92 x ptr] zeroinitializer, align 8

; Each entry maps hash of an assembly name to an index into the `assembly_image_cache` array
@assembly_image_cache_hashes = dso_local local_unnamed_addr constant [276 x i64] [
	i64 u0x0071cf2d27b7d61e, ; 0: lib_Xamarin.AndroidX.SwipeRefreshLayout.dll.so => 41
	i64 u0x02123411c4e01926, ; 1: lib_Xamarin.AndroidX.Navigation.Runtime.dll.so => 37
	i64 u0x02abedc11addc1ed, ; 2: lib_Mono.Android.Runtime.dll.so => 90
	i64 u0x032267b2a94db371, ; 3: lib_Xamarin.AndroidX.AppCompat.dll.so => 20
	i64 u0x0363ac97a4cb84e6, ; 4: SQLitePCLRaw.provider.e_sqlite3.dll => 18
	i64 u0x0517ef04e06e9f76, ; 5: System.Net.Primitives => 67
	i64 u0x0565d18c6da3de38, ; 6: Xamarin.AndroidX.RecyclerView => 39
	i64 u0x0581db89237110e9, ; 7: lib_System.Collections.dll.so => 53
	i64 u0x05989cb940b225a9, ; 8: Microsoft.Maui.dll => 11
	i64 u0x0680a433c781bb3d, ; 9: Xamarin.AndroidX.Collection.Jvm => 23
	i64 u0x07dcdc7460a0c5e4, ; 10: System.Collections.NonGeneric => 51
	i64 u0x08f3c9788ee2153c, ; 11: Xamarin.AndroidX.DrawerLayout => 28
	i64 u0x0919c28b89381a0b, ; 12: lib_Microsoft.Extensions.Options.dll.so => 7
	i64 u0x092266563089ae3e, ; 13: lib_System.Collections.NonGeneric.dll.so => 51
	i64 u0x09d144a7e214d457, ; 14: System.Security.Cryptography => 78
	i64 u0x0b6aff547b84fbe9, ; 15: Xamarin.KotlinX.Serialization.Core.Jvm => 48
	i64 u0x0be2e1f8ce4064ed, ; 16: Xamarin.AndroidX.ViewPager => 43
	i64 u0x0c59ad9fbbd43abe, ; 17: Mono.Android => 91
	i64 u0x0c7790f60165fc06, ; 18: lib_Microsoft.Maui.Essentials.dll.so => 12
	i64 u0x102a31b45304b1da, ; 19: Xamarin.AndroidX.CustomView => 27
	i64 u0x10f6cfcbcf801616, ; 20: System.IO.Compression.Brotli => 60
	i64 u0x125b7f94acb989db, ; 21: Xamarin.AndroidX.RecyclerView.dll => 39
	i64 u0x13f1e5e209e91af4, ; 22: lib_Java.Interop.dll.so => 89
	i64 u0x143d8ea60a6a4011, ; 23: Microsoft.Extensions.DependencyInjection.Abstractions => 4
	i64 u0x17125c9a85b4929f, ; 24: lib_netstandard.dll.so => 87
	i64 u0x17f9358913beb16a, ; 25: System.Text.Encodings.Web => 79
	i64 u0x18402a709e357f3b, ; 26: lib_Xamarin.KotlinX.Serialization.Core.Jvm.dll.so => 48
	i64 u0x19232f2de96cef5c, ; 27: DailyExpenseTracker.dll => 49
	i64 u0x1a91866a319e9259, ; 28: lib_System.Collections.Concurrent.dll.so => 50
	i64 u0x1aac34d1917ba5d3, ; 29: lib_System.dll.so => 86
	i64 u0x1c753b5ff15bce1b, ; 30: Mono.Android.Runtime.dll => 90
	i64 u0x1e3d87657e9659bc, ; 31: Xamarin.AndroidX.Navigation.UI => 38
	i64 u0x1ed8fcce5e9b50a0, ; 32: Microsoft.Extensions.Options.dll => 7
	i64 u0x209375905fcc1bad, ; 33: lib_System.IO.Compression.Brotli.dll.so => 60
	i64 u0x2174319c0d835bc9, ; 34: System.Runtime => 77
	i64 u0x237be844f1f812c7, ; 35: System.Threading.Thread.dll => 82
	i64 u0x2407aef2bbe8fadf, ; 36: System.Console => 57
	i64 u0x240abe014b27e7d3, ; 37: Xamarin.AndroidX.Core.dll => 25
	i64 u0x24e18fc7e12bb7d7, ; 38: DailyExpenseTracker => 49
	i64 u0x25a0a7eff76ea08e, ; 39: SQLitePCLRaw.batteries_v2.dll => 15
	i64 u0x2662c629b96b0b30, ; 40: lib_Xamarin.Kotlin.StdLib.dll.so => 46
	i64 u0x268c1439f13bcc29, ; 41: lib_Microsoft.Extensions.Primitives.dll.so => 8
	i64 u0x27b410442fad6cf1, ; 42: Java.Interop.dll => 89
	i64 u0x2801845a2c71fbfb, ; 43: System.Net.Primitives.dll => 67
	i64 u0x2af298f63581d886, ; 44: System.Text.RegularExpressions.dll => 81
	i64 u0x2afc1c4f898552ee, ; 45: lib_System.Formats.Asn1.dll.so => 59
	i64 u0x2d169d318a968379, ; 46: System.Threading.dll => 83
	i64 u0x2db915caf23548d2, ; 47: System.Text.Json.dll => 80
	i64 u0x2f2e98e1c89b1aff, ; 48: System.Xml.ReaderWriter => 85
	i64 u0x309ee9eeec09a71e, ; 49: lib_Xamarin.AndroidX.Fragment.dll.so => 29
	i64 u0x31195fef5d8fb552, ; 50: _Microsoft.Android.Resource.Designer.dll => 0
	i64 u0x32243413e774362a, ; 51: Xamarin.AndroidX.CardView.dll => 22
	i64 u0x32aa989ff07a84ff, ; 52: lib_System.Xml.ReaderWriter.dll.so => 85
	i64 u0x34dfd74fe2afcf37, ; 53: Microsoft.Maui => 11
	i64 u0x3508234247f48404, ; 54: Microsoft.Maui.Controls => 9
	i64 u0x3549870798b4cd30, ; 55: lib_Xamarin.AndroidX.ViewPager2.dll.so => 44
	i64 u0x355282fc1c909694, ; 56: Microsoft.Extensions.Configuration => 1
	i64 u0x385c17636bb6fe6e, ; 57: Xamarin.AndroidX.CustomView.dll => 27
	i64 u0x393c226616977fdb, ; 58: lib_Xamarin.AndroidX.ViewPager.dll.so => 43
	i64 u0x3c7c495f58ac5ee9, ; 59: Xamarin.Kotlin.StdLib => 46
	i64 u0x3d9c2a242b040a50, ; 60: lib_Xamarin.AndroidX.Core.dll.so => 25
	i64 u0x3da7781d6333a8fe, ; 61: SQLitePCLRaw.batteries_v2 => 15
	i64 u0x407a10bb4bf95829, ; 62: lib_Xamarin.AndroidX.Navigation.Common.dll.so => 35
	i64 u0x41cab042be111c34, ; 63: lib_Xamarin.AndroidX.AppCompat.AppCompatResources.dll.so => 21
	i64 u0x43375950ec7c1b6a, ; 64: netstandard.dll => 87
	i64 u0x434c4e1d9284cdae, ; 65: Mono.Android.dll => 91
	i64 u0x4515080865a951a5, ; 66: Xamarin.Kotlin.StdLib.dll => 46
	i64 u0x49e952f19a4e2022, ; 67: System.ObjectModel => 70
	i64 u0x49f9e6948a8131e4, ; 68: lib_Xamarin.AndroidX.VersionedParcelable.dll.so => 42
	i64 u0x4a5667b2462a664b, ; 69: lib_Xamarin.AndroidX.Navigation.UI.dll.so => 38
	i64 u0x4b7b6532ded934b7, ; 70: System.Text.Json => 80
	i64 u0x4cc5f15266470798, ; 71: lib_Xamarin.AndroidX.Loader.dll.so => 34
	i64 u0x4d479f968a05e504, ; 72: System.Linq.Expressions.dll => 63
	i64 u0x4d55a010ffc4faff, ; 73: System.Private.Xml => 72
	i64 u0x4d95fccc1f67c7ca, ; 74: System.Runtime.Loader.dll => 75
	i64 u0x4dd9247f1d2c3235, ; 75: Xamarin.AndroidX.Loader.dll => 34
	i64 u0x4e32f00cb0937401, ; 76: Mono.Android.Runtime => 90
	i64 u0x4fd5f3ee53d0a4f0, ; 77: SQLitePCLRaw.lib.e_sqlite3.android => 17
	i64 u0x5037f0be3c28c7a3, ; 78: lib_Microsoft.Maui.Controls.dll.so => 9
	i64 u0x5131bbe80989093f, ; 79: Xamarin.AndroidX.Lifecycle.ViewModel.Android.dll => 32
	i64 u0x526ce79eb8e90527, ; 80: lib_System.Net.Primitives.dll.so => 67
	i64 u0x529ffe06f39ab8db, ; 81: Xamarin.AndroidX.Core => 25
	i64 u0x52ff996554dbf352, ; 82: Microsoft.Maui.Graphics => 13
	i64 u0x53be1038a61e8d44, ; 83: System.Runtime.InteropServices.RuntimeInformation.dll => 73
	i64 u0x54795225dd1587af, ; 84: lib_System.Runtime.dll.so => 77
	i64 u0x556e8b63b660ab8b, ; 85: Xamarin.AndroidX.Lifecycle.Common.Jvm.dll => 30
	i64 u0x5588627c9a108ec9, ; 86: System.Collections.Specialized => 52
	i64 u0x571c5cfbec5ae8e2, ; 87: System.Private.Uri => 71
	i64 u0x578cd35c91d7b347, ; 88: lib_SQLitePCLRaw.core.dll.so => 16
	i64 u0x579a06fed6eec900, ; 89: System.Private.CoreLib.dll => 88
	i64 u0x57c542c14049b66d, ; 90: System.Diagnostics.DiagnosticSource => 58
	i64 u0x58688d9af496b168, ; 91: Microsoft.Extensions.DependencyInjection.dll => 3
	i64 u0x5a89a886ae30258d, ; 92: lib_Xamarin.AndroidX.CoordinatorLayout.dll.so => 24
	i64 u0x5a8f6699f4a1caa9, ; 93: lib_System.Threading.dll.so => 83
	i64 u0x5ae9cd33b15841bf, ; 94: System.ComponentModel => 56
	i64 u0x5c393624b8176517, ; 95: lib_Microsoft.Extensions.Logging.dll.so => 5
	i64 u0x5db0cbbd1028510e, ; 96: lib_System.Runtime.InteropServices.dll.so => 74
	i64 u0x5db30905d3e5013b, ; 97: Xamarin.AndroidX.Collection.Jvm.dll => 23
	i64 u0x5e467bc8f09ad026, ; 98: System.Collections.Specialized.dll => 52
	i64 u0x5ea92fdb19ec8c4c, ; 99: System.Text.Encodings.Web.dll => 79
	i64 u0x5eb8046dd40e9ac3, ; 100: System.ComponentModel.Primitives => 54
	i64 u0x5f36ccf5c6a57e24, ; 101: System.Xml.ReaderWriter.dll => 85
	i64 u0x5f7399e166075632, ; 102: lib_SQLitePCLRaw.lib.e_sqlite3.android.dll.so => 17
	i64 u0x609f4b7b63d802d4, ; 103: lib_Microsoft.Extensions.DependencyInjection.dll.so => 3
	i64 u0x60cd4e33d7e60134, ; 104: Xamarin.KotlinX.Coroutines.Core.Jvm => 47
	i64 u0x60f62d786afcf130, ; 105: System.Memory => 65
	i64 u0x61be8d1299194243, ; 106: Microsoft.Maui.Controls.Xaml => 10
	i64 u0x61d88f399afb2f45, ; 107: lib_System.Runtime.Loader.dll.so => 75
	i64 u0x622eef6f9e59068d, ; 108: System.Private.CoreLib => 88
	i64 u0x6400f68068c1e9f1, ; 109: Xamarin.Google.Android.Material.dll => 45
	i64 u0x65ecac39144dd3cc, ; 110: Microsoft.Maui.Controls.dll => 9
	i64 u0x65ece51227bfa724, ; 111: lib_System.Runtime.Numerics.dll.so => 76
	i64 u0x6692e924eade1b29, ; 112: lib_System.Console.dll.so => 57
	i64 u0x66a4e5c6a3fb0bae, ; 113: lib_Xamarin.AndroidX.Lifecycle.ViewModel.Android.dll.so => 32
	i64 u0x66d13304ce1a3efa, ; 114: Xamarin.AndroidX.CursorAdapter => 26
	i64 u0x68fbbbe2eb455198, ; 115: System.Formats.Asn1 => 59
	i64 u0x699dffb2427a2d71, ; 116: SQLitePCLRaw.lib.e_sqlite3.android.dll => 17
	i64 u0x6a4d7577b2317255, ; 117: System.Runtime.InteropServices.dll => 74
	i64 u0x6d12bfaa99c72b1f, ; 118: lib_Microsoft.Maui.Graphics.dll.so => 13
	i64 u0x6d79993361e10ef2, ; 119: Microsoft.Extensions.Primitives => 8
	i64 u0x6d86d56b84c8eb71, ; 120: lib_Xamarin.AndroidX.CursorAdapter.dll.so => 26
	i64 u0x6d9bea6b3e895cf7, ; 121: Microsoft.Extensions.Primitives.dll => 8
	i64 u0x6e25a02c3833319a, ; 122: lib_Xamarin.AndroidX.Navigation.Fragment.dll.so => 36
	i64 u0x6fd2265da78b93a4, ; 123: lib_Microsoft.Maui.dll.so => 11
	i64 u0x71ad672adbe48f35, ; 124: System.ComponentModel.Primitives.dll => 54
	i64 u0x73e4ce94e2eb6ffc, ; 125: lib_System.Memory.dll.so => 65
	i64 u0x755a91767330b3d4, ; 126: lib_Microsoft.Extensions.Configuration.dll.so => 1
	i64 u0x76012e7334db86e5, ; 127: lib_Xamarin.AndroidX.SavedState.dll.so => 40
	i64 u0x76ca07b878f44da0, ; 128: System.Runtime.Numerics.dll => 76
	i64 u0x78a45e51311409b6, ; 129: Xamarin.AndroidX.Fragment.dll => 29
	i64 u0x7bef86a4335c4870, ; 130: System.ComponentModel.TypeConverter => 55
	i64 u0x7d8ee2bdc8e3aad1, ; 131: System.Numerics.Vectors => 69
	i64 u0x7dfc3d6d9d8d7b70, ; 132: System.Collections => 53
	i64 u0x7e946809d6008ef2, ; 133: lib_System.ObjectModel.dll.so => 70
	i64 u0x7ecc13347c8fd849, ; 134: lib_System.ComponentModel.dll.so => 56
	i64 u0x7f00ddd9b9ca5a13, ; 135: Xamarin.AndroidX.ViewPager.dll => 43
	i64 u0x7f9351cd44b1273f, ; 136: Microsoft.Extensions.Configuration.Abstractions => 2
	i64 u0x7fbd557c99b3ce6f, ; 137: lib_Xamarin.AndroidX.Lifecycle.LiveData.Core.dll.so => 31
	i64 u0x80fa55b6d1b0be99, ; 138: SQLitePCLRaw.provider.e_sqlite3 => 18
	i64 u0x812c069d5cdecc17, ; 139: System.dll => 86
	i64 u0x8277f2be6b5ce05f, ; 140: Xamarin.AndroidX.AppCompat => 20
	i64 u0x828f06563b30bc50, ; 141: lib_Xamarin.AndroidX.CardView.dll.so => 22
	i64 u0x83144699b312ad81, ; 142: SQLite-net.dll => 14
	i64 u0x86b3e00c36b84509, ; 143: Microsoft.Extensions.Configuration.dll => 1
	i64 u0x87c69b87d9283884, ; 144: lib_System.Threading.Thread.dll.so => 82
	i64 u0x87f6569b25707834, ; 145: System.IO.Compression.Brotli.dll => 60
	i64 u0x8842b3a5d2d3fb36, ; 146: Microsoft.Maui.Essentials => 12
	i64 u0x88bda98e0cffb7a9, ; 147: lib_Xamarin.KotlinX.Coroutines.Core.Jvm.dll.so => 47
	i64 u0x8930322c7bd8f768, ; 148: netstandard => 87
	i64 u0x897a606c9e39c75f, ; 149: lib_System.ComponentModel.Primitives.dll.so => 54
	i64 u0x89c5188089ec2cd5, ; 150: lib_System.Runtime.InteropServices.RuntimeInformation.dll.so => 73
	i64 u0x8ad229ea26432ee2, ; 151: Xamarin.AndroidX.Loader => 34
	i64 u0x8b4ff5d0fdd5faa1, ; 152: lib_System.Diagnostics.DiagnosticSource.dll.so => 58
	i64 u0x8d0f420977c2c1c7, ; 153: Xamarin.AndroidX.CursorAdapter.dll => 26
	i64 u0x8d7b8ab4b3310ead, ; 154: System.Threading => 83
	i64 u0x8da188285aadfe8e, ; 155: System.Collections.Concurrent => 50
	i64 u0x8ed807bfe9858dfc, ; 156: Xamarin.AndroidX.Navigation.Common => 35
	i64 u0x8ef9414937d93a0a, ; 157: SQLitePCLRaw.core.dll => 16
	i64 u0x8fd27d934d7b3a55, ; 158: SQLitePCLRaw.core => 16
	i64 u0x903101b46fb73a04, ; 159: _Microsoft.Android.Resource.Designer => 0
	i64 u0x90393bd4865292f3, ; 160: lib_System.IO.Compression.dll.so => 61
	i64 u0x90634f86c5ebe2b5, ; 161: Xamarin.AndroidX.Lifecycle.ViewModel.Android => 32
	i64 u0x907b636704ad79ef, ; 162: lib_Microsoft.Maui.Controls.Xaml.dll.so => 10
	i64 u0x91418dc638b29e68, ; 163: lib_Xamarin.AndroidX.CustomView.dll.so => 27
	i64 u0x9157bd523cd7ed36, ; 164: lib_System.Text.Json.dll.so => 80
	i64 u0x91a74f07b30d37e2, ; 165: System.Linq.dll => 64
	i64 u0x944077d8ca3c6580, ; 166: System.IO.Compression.dll => 61
	i64 u0x978be80e5210d31b, ; 167: Microsoft.Maui.Graphics.dll => 13
	i64 u0x97b8c771ea3e4220, ; 168: System.ComponentModel.dll => 56
	i64 u0x97e144c9d3c6976e, ; 169: System.Collections.Concurrent.dll => 50
	i64 u0x991d510397f92d9d, ; 170: System.Linq.Expressions => 63
	i64 u0x99a00ca5270c6878, ; 171: Xamarin.AndroidX.Navigation.Runtime => 37
	i64 u0x9d5dbcf5a48583fe, ; 172: lib_Xamarin.AndroidX.Activity.dll.so => 19
	i64 u0x9d74dee1a7725f34, ; 173: Microsoft.Extensions.Configuration.Abstractions.dll => 2
	i64 u0x9eaf1efdf6f7267e, ; 174: Xamarin.AndroidX.Navigation.Common.dll => 35
	i64 u0x9ef542cf1f78c506, ; 175: Xamarin.AndroidX.Lifecycle.LiveData.Core => 31
	i64 u0xa0d8259f4cc284ec, ; 176: lib_System.Security.Cryptography.dll.so => 78
	i64 u0xa1440773ee9d341e, ; 177: Xamarin.Google.Android.Material => 45
	i64 u0xa1b9d7c27f47219f, ; 178: Xamarin.AndroidX.Navigation.UI.dll => 38
	i64 u0xa2572680829d2c7c, ; 179: System.IO.Pipelines.dll => 62
	i64 u0xa5e599d1e0524750, ; 180: System.Numerics.Vectors.dll => 69
	i64 u0xa5f1ba49b85dd355, ; 181: System.Security.Cryptography.dll => 78
	i64 u0xa67dbee13e1df9ca, ; 182: Xamarin.AndroidX.SavedState.dll => 40
	i64 u0xa68a420042bb9b1f, ; 183: Xamarin.AndroidX.DrawerLayout.dll => 28
	i64 u0xa78ce3745383236a, ; 184: Xamarin.AndroidX.Lifecycle.Common.Jvm => 30
	i64 u0xaa2219c8e3449ff5, ; 185: Microsoft.Extensions.Logging.Abstractions => 6
	i64 u0xaa443ac34067eeef, ; 186: System.Private.Xml.dll => 72
	i64 u0xaa52de307ef5d1dd, ; 187: System.Net.Http => 66
	i64 u0xaaaf86367285a918, ; 188: Microsoft.Extensions.DependencyInjection.Abstractions.dll => 4
	i64 u0xab9c1b2687d86b0b, ; 189: lib_System.Linq.Expressions.dll.so => 63
	i64 u0xac2af3fa195a15ce, ; 190: System.Runtime.Numerics => 76
	i64 u0xac5376a2a538dc10, ; 191: Xamarin.AndroidX.Lifecycle.LiveData.Core.dll => 31
	i64 u0xadbb53caf78a79d2, ; 192: System.Web.HttpUtility => 84
	i64 u0xadc90ab061a9e6e4, ; 193: System.ComponentModel.TypeConverter.dll => 55
	i64 u0xae282bcd03739de7, ; 194: Java.Interop => 89
	i64 u0xae53579c90db1107, ; 195: System.ObjectModel.dll => 70
	i64 u0xae7ea18c61eef394, ; 196: SQLite-net => 14
	i64 u0xafe29f45095518e7, ; 197: lib_Xamarin.AndroidX.Lifecycle.ViewModelSavedState.dll.so => 33
	i64 u0xb220631954820169, ; 198: System.Text.RegularExpressions => 81
	i64 u0xb3f0a0fcda8d3ebc, ; 199: Xamarin.AndroidX.CardView => 22
	i64 u0xb4bd7015ecee9d86, ; 200: System.IO.Pipelines => 62
	i64 u0xb5c7fcdafbc67ee4, ; 201: Microsoft.Extensions.Logging.Abstractions.dll => 6
	i64 u0xb81a2c6e0aee50fe, ; 202: lib_System.Private.CoreLib.dll.so => 88
	i64 u0xba48785529705af9, ; 203: System.Collections.dll => 53
	i64 u0xbc22a245dab70cb4, ; 204: lib_SQLitePCLRaw.provider.e_sqlite3.dll.so => 18
	i64 u0xbd0e2c0d55246576, ; 205: System.Net.Http.dll => 66
	i64 u0xbd437a2cdb333d0d, ; 206: Xamarin.AndroidX.ViewPager2 => 44
	i64 u0xbee38d4a88835966, ; 207: Xamarin.AndroidX.AppCompat.AppCompatResources => 21
	i64 u0xc0d928351ab5ca77, ; 208: System.Console.dll => 57
	i64 u0xc12b8b3afa48329c, ; 209: lib_System.Linq.dll.so => 64
	i64 u0xc1ff9ae3cdb6e1e6, ; 210: Xamarin.AndroidX.Activity.dll => 19
	i64 u0xc2bcfec99f69365e, ; 211: Xamarin.AndroidX.ViewPager2.dll => 44
	i64 u0xc4d3858ed4d08512, ; 212: Xamarin.AndroidX.Lifecycle.ViewModelSavedState.dll => 33
	i64 u0xc50fded0ded1418c, ; 213: lib_System.ComponentModel.TypeConverter.dll.so => 55
	i64 u0xc519125d6bc8fb11, ; 214: lib_System.Net.Requests.dll.so => 68
	i64 u0xc5293b19e4dc230e, ; 215: Xamarin.AndroidX.Navigation.Fragment => 36
	i64 u0xc5325b2fcb37446f, ; 216: lib_System.Private.Xml.dll.so => 72
	i64 u0xc5a0f4b95a699af7, ; 217: lib_System.Private.Uri.dll.so => 71
	i64 u0xc7ce851898a4548e, ; 218: lib_System.Web.HttpUtility.dll.so => 84
	i64 u0xc858a28d9ee5a6c5, ; 219: lib_System.Collections.Specialized.dll.so => 52
	i64 u0xcbd4fdd9cef4a294, ; 220: lib__Microsoft.Android.Resource.Designer.dll.so => 0
	i64 u0xcc2876b32ef2794c, ; 221: lib_System.Text.RegularExpressions.dll.so => 81
	i64 u0xcc5c3bb714c4561e, ; 222: Xamarin.KotlinX.Coroutines.Core.Jvm.dll => 47
	i64 u0xcc76886e09b88260, ; 223: Xamarin.KotlinX.Serialization.Core.Jvm.dll => 48
	i64 u0xcd10a42808629144, ; 224: System.Net.Requests => 68
	i64 u0xcdd0c48b6937b21c, ; 225: Xamarin.AndroidX.SwipeRefreshLayout => 41
	i64 u0xcf23d8093f3ceadf, ; 226: System.Diagnostics.DiagnosticSource.dll => 58
	i64 u0xd1194e1d8a8de83c, ; 227: lib_Xamarin.AndroidX.Lifecycle.Common.Jvm.dll.so => 30
	i64 u0xd1c1063008ecb795, ; 228: lib_DailyExpenseTracker.dll.so => 49
	i64 u0xd333d0af9e423810, ; 229: System.Runtime.InteropServices => 74
	i64 u0xd3426d966bb704f5, ; 230: Xamarin.AndroidX.AppCompat.AppCompatResources.dll => 21
	i64 u0xd3651b6fc3125825, ; 231: System.Private.Uri.dll => 71
	i64 u0xd373685349b1fe8b, ; 232: Microsoft.Extensions.Logging.dll => 5
	i64 u0xd4645626dffec99d, ; 233: lib_Microsoft.Extensions.DependencyInjection.Abstractions.dll.so => 4
	i64 u0xd5507e11a2b2839f, ; 234: Xamarin.AndroidX.Lifecycle.ViewModelSavedState => 33
	i64 u0xd6694f8359737e4e, ; 235: Xamarin.AndroidX.SavedState => 40
	i64 u0xd6d21782156bc35b, ; 236: Xamarin.AndroidX.SwipeRefreshLayout.dll => 41
	i64 u0xd72329819cbbbc44, ; 237: lib_Microsoft.Extensions.Configuration.Abstractions.dll.so => 2
	i64 u0xd7b3764ada9d341d, ; 238: lib_Microsoft.Extensions.Logging.Abstractions.dll.so => 6
	i64 u0xd7f0088bc5ad71f2, ; 239: Xamarin.AndroidX.VersionedParcelable => 42
	i64 u0xda1dfa4c534a9251, ; 240: Microsoft.Extensions.DependencyInjection => 3
	i64 u0xdad05a11827959a3, ; 241: System.Collections.NonGeneric.dll => 51
	i64 u0xdbf9607a441b4505, ; 242: System.Linq => 64
	i64 u0xdce2c53525640bf3, ; 243: Microsoft.Extensions.Logging => 5
	i64 u0xdd2b722d78ef5f43, ; 244: System.Runtime.dll => 77
	i64 u0xdd67031857c72f96, ; 245: lib_System.Text.Encodings.Web.dll.so => 79
	i64 u0xe0142572c095a480, ; 246: Xamarin.AndroidX.AppCompat.dll => 20
	i64 u0xe02f89350ec78051, ; 247: Xamarin.AndroidX.CoordinatorLayout.dll => 24
	i64 u0xe192a588d4410686, ; 248: lib_System.IO.Pipelines.dll.so => 62
	i64 u0xe1a08bd3fa539e0d, ; 249: System.Runtime.Loader => 75
	i64 u0xe2420585aeceb728, ; 250: System.Net.Requests.dll => 68
	i64 u0xe3a586956771a0ed, ; 251: lib_SQLite-net.dll.so => 14
	i64 u0xe5434e8a119ceb69, ; 252: lib_Mono.Android.dll.so => 91
	i64 u0xed19c616b3fcb7eb, ; 253: Xamarin.AndroidX.VersionedParcelable.dll => 42
	i64 u0xedc632067fb20ff3, ; 254: System.Memory.dll => 65
	i64 u0xedc8e4ca71a02a8b, ; 255: Xamarin.AndroidX.Navigation.Runtime.dll => 37
	i64 u0xeeb7ebb80150501b, ; 256: lib_Xamarin.AndroidX.Collection.Jvm.dll.so => 23
	i64 u0xef72742e1bcca27a, ; 257: Microsoft.Maui.Essentials.dll => 12
	i64 u0xefec0b7fdc57ec42, ; 258: Xamarin.AndroidX.Activity => 19
	i64 u0xf11b621fc87b983f, ; 259: Microsoft.Maui.Controls.Xaml.dll => 10
	i64 u0xf1c4b4005493d871, ; 260: System.Formats.Asn1.dll => 59
	i64 u0xf37221fda4ef8830, ; 261: lib_Xamarin.Google.Android.Material.dll.so => 45
	i64 u0xf3ddfe05336abf29, ; 262: System => 86
	i64 u0xf4c1dd70a5496a17, ; 263: System.IO.Compression => 61
	i64 u0xf6077741019d7428, ; 264: Xamarin.AndroidX.CoordinatorLayout => 24
	i64 u0xf7e2cac4c45067b3, ; 265: lib_System.Numerics.Vectors.dll.so => 69
	i64 u0xf8e045dc345b2ea3, ; 266: lib_Xamarin.AndroidX.RecyclerView.dll.so => 39
	i64 u0xf915dc29808193a1, ; 267: System.Web.HttpUtility.dll => 84
	i64 u0xf9eec5bb3a6aedc6, ; 268: Microsoft.Extensions.Options => 7
	i64 u0xfa645d91e9fc4cba, ; 269: System.Threading.Thread => 82
	i64 u0xfb022853d73b7fa5, ; 270: lib_SQLitePCLRaw.batteries_v2.dll.so => 15
	i64 u0xfbf0a31c9fc34bc4, ; 271: lib_System.Net.Http.dll.so => 66
	i64 u0xfc719aec26adf9d9, ; 272: Xamarin.AndroidX.Navigation.Fragment.dll => 36
	i64 u0xfd22f00870e40ae0, ; 273: lib_Xamarin.AndroidX.DrawerLayout.dll.so => 28
	i64 u0xfd49b3c1a76e2748, ; 274: System.Runtime.InteropServices.RuntimeInformation => 73
	i64 u0xfd583f7657b6a1cb ; 275: Xamarin.AndroidX.Fragment => 29
], align 8

@assembly_image_cache_indices = dso_local local_unnamed_addr constant [276 x i32] [
	i32 41, i32 37, i32 90, i32 20, i32 18, i32 67, i32 39, i32 53,
	i32 11, i32 23, i32 51, i32 28, i32 7, i32 51, i32 78, i32 48,
	i32 43, i32 91, i32 12, i32 27, i32 60, i32 39, i32 89, i32 4,
	i32 87, i32 79, i32 48, i32 49, i32 50, i32 86, i32 90, i32 38,
	i32 7, i32 60, i32 77, i32 82, i32 57, i32 25, i32 49, i32 15,
	i32 46, i32 8, i32 89, i32 67, i32 81, i32 59, i32 83, i32 80,
	i32 85, i32 29, i32 0, i32 22, i32 85, i32 11, i32 9, i32 44,
	i32 1, i32 27, i32 43, i32 46, i32 25, i32 15, i32 35, i32 21,
	i32 87, i32 91, i32 46, i32 70, i32 42, i32 38, i32 80, i32 34,
	i32 63, i32 72, i32 75, i32 34, i32 90, i32 17, i32 9, i32 32,
	i32 67, i32 25, i32 13, i32 73, i32 77, i32 30, i32 52, i32 71,
	i32 16, i32 88, i32 58, i32 3, i32 24, i32 83, i32 56, i32 5,
	i32 74, i32 23, i32 52, i32 79, i32 54, i32 85, i32 17, i32 3,
	i32 47, i32 65, i32 10, i32 75, i32 88, i32 45, i32 9, i32 76,
	i32 57, i32 32, i32 26, i32 59, i32 17, i32 74, i32 13, i32 8,
	i32 26, i32 8, i32 36, i32 11, i32 54, i32 65, i32 1, i32 40,
	i32 76, i32 29, i32 55, i32 69, i32 53, i32 70, i32 56, i32 43,
	i32 2, i32 31, i32 18, i32 86, i32 20, i32 22, i32 14, i32 1,
	i32 82, i32 60, i32 12, i32 47, i32 87, i32 54, i32 73, i32 34,
	i32 58, i32 26, i32 83, i32 50, i32 35, i32 16, i32 16, i32 0,
	i32 61, i32 32, i32 10, i32 27, i32 80, i32 64, i32 61, i32 13,
	i32 56, i32 50, i32 63, i32 37, i32 19, i32 2, i32 35, i32 31,
	i32 78, i32 45, i32 38, i32 62, i32 69, i32 78, i32 40, i32 28,
	i32 30, i32 6, i32 72, i32 66, i32 4, i32 63, i32 76, i32 31,
	i32 84, i32 55, i32 89, i32 70, i32 14, i32 33, i32 81, i32 22,
	i32 62, i32 6, i32 88, i32 53, i32 18, i32 66, i32 44, i32 21,
	i32 57, i32 64, i32 19, i32 44, i32 33, i32 55, i32 68, i32 36,
	i32 72, i32 71, i32 84, i32 52, i32 0, i32 81, i32 47, i32 48,
	i32 68, i32 41, i32 58, i32 30, i32 49, i32 74, i32 21, i32 71,
	i32 5, i32 4, i32 33, i32 40, i32 41, i32 2, i32 6, i32 42,
	i32 3, i32 51, i32 64, i32 5, i32 77, i32 79, i32 20, i32 24,
	i32 62, i32 75, i32 68, i32 14, i32 91, i32 42, i32 65, i32 37,
	i32 23, i32 12, i32 19, i32 10, i32 59, i32 45, i32 86, i32 61,
	i32 24, i32 69, i32 39, i32 84, i32 7, i32 82, i32 15, i32 66,
	i32 36, i32 28, i32 73, i32 29
], align 4

@marshal_methods_number_of_classes = dso_local local_unnamed_addr constant i32 0, align 4

@marshal_methods_class_cache = dso_local local_unnamed_addr global [0 x %struct.MarshalMethodsManagedClass] zeroinitializer, align 8

; Names of classes in which marshal methods reside
@mm_class_names = dso_local local_unnamed_addr constant [0 x ptr] zeroinitializer, align 8

@mm_method_names = dso_local local_unnamed_addr constant [1 x %struct.MarshalMethodName] [
	%struct.MarshalMethodName {
		i64 u0x0000000000000000, ; name: 
		ptr @.MarshalMethodName.0_name; char* name
	} ; 0
], align 8

; get_function_pointer (uint32_t mono_image_index, uint32_t class_index, uint32_t method_token, void*& target_ptr)
@get_function_pointer = internal dso_local unnamed_addr global ptr null, align 8

; Functions

; Function attributes: memory(write, argmem: none, inaccessiblemem: none) "min-legal-vector-width"="0" mustprogress nofree norecurse nosync "no-trapping-math"="true" nounwind "stack-protector-buffer-size"="8" uwtable willreturn
define void @xamarin_app_init(ptr nocapture noundef readnone %env, ptr noundef %fn) local_unnamed_addr #0
{
	%fnIsNull = icmp eq ptr %fn, null
	br i1 %fnIsNull, label %1, label %2

1: ; preds = %0
	%putsResult = call noundef i32 @puts(ptr @.str.0)
	call void @abort()
	unreachable 

2: ; preds = %1, %0
	store ptr %fn, ptr @get_function_pointer, align 8, !tbaa !3
	ret void
}

; Strings
@.str.0 = private unnamed_addr constant [40 x i8] c"get_function_pointer MUST be specified\0A\00", align 1

;MarshalMethodName
@.MarshalMethodName.0_name = private unnamed_addr constant [1 x i8] c"\00", align 1

; External functions

; Function attributes: noreturn "no-trapping-math"="true" nounwind "stack-protector-buffer-size"="8"
declare void @abort() local_unnamed_addr #2

; Function attributes: nofree nounwind
declare noundef i32 @puts(ptr noundef) local_unnamed_addr #1
attributes #0 = { memory(write, argmem: none, inaccessiblemem: none) "min-legal-vector-width"="0" mustprogress nofree norecurse nosync "no-trapping-math"="true" nounwind "stack-protector-buffer-size"="8" "target-cpu"="generic" "target-features"="+fix-cortex-a53-835769,+neon,+outline-atomics,+v8a" uwtable willreturn }
attributes #1 = { nofree nounwind }
attributes #2 = { noreturn "no-trapping-math"="true" nounwind "stack-protector-buffer-size"="8" "target-cpu"="generic" "target-features"="+fix-cortex-a53-835769,+neon,+outline-atomics,+v8a" }

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
