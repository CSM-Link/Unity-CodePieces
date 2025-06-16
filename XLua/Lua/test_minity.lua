Include("Common")

local output_file_prefix = "lua_test_minity"
local log_prefix = output_file_prefix .. " "
print("g_test_config" .. Config.g_test_config)

function register()
    Common.ClearRegisteredFunction()

    Common.RegisterFunction(Common.PrintGlobal)
    Common.RegisterFunction(Common.PrintCS)
    Common.RegisterFunction(PrintEnv)
    Common.RegisterFunction(RealtimeReloadFunc1)
end

function onDestroy()
    Common.ClearRegisteredFunction()
end

function RealtimeReloadFunc1()
    print(log_prefix.."RealtimeReloadFunc1 called")
    print(log_prefix.."test")
end

-- 打印当前环境表到文件
function PrintEnv()
    print(Common.g_testint)
    print(log_prefix.."PrintEnv started ")
    local file, err = io.open(output_file_prefix .. "_env.txt", "w")
    if not file then
        print(log_prefix.."Error opening file: " .. err)
        return
    end

    for k, v in pairs(_ENV) do
        file:write(k, '=', tostring(v), '\n')
    end
    file:close()
    print(log_prefix.."PrintEnv finished ")
end


