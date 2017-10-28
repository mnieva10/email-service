git init 
git submodule add -b master git@github.com:Sovos-Compliance/Castle.Core-READONLY.git __submodules/Castle.Core-READONLY 
git submodule add -b development git@bucket.convey.com:Sovos/crypt-utility.git __submodules/crypt-utility 
git submodule add -b convey-ibatis-1-maintenance git@bucket.convey.com:Sovos/mybatis-mybatisnet.git __submodules/mybatis-mybatisnet 
git submodule add -b v2 git@github.com:Sovos-Compliance/service-bus-csharp.git __submodules/service-bus-csharp 
git submodule add -b development git@bucket.convey.com:Sovos/svcbus-common-model.git __submodules/svcbus-common-model 
git submodule update --init --recursive --remote
git -C __submodules/Castle.Core-READONLY checkout 98d32d968ae306c0e7f5a766e3200a89ec7aa655 
git -C __submodules/crypt-utility checkout 51bba65b34473494448ca247a68bf47f3c272c96 
git -C __submodules/mybatis-mybatisnet checkout c5893fba62d3d5135a3f2b78594bee913428aa80 
git -C __submodules/service-bus-csharp checkout d4102d83f823a4234c869fb14ceababedb0045bc 
git -C __submodules/svcbus-common-model checkout 8577df900093b4a95b5d36768c6f43c1f9e4efa5 
git -C __submodules/service-bus-csharp/gc-helper checkout 14087de5e908c8e08bf7a29de5a8de4d024efcd1