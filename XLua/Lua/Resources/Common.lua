
-- ------------------------------------------------------
--- C# Function Aliases

RegisterFunction = CS.LuaCSharp.CSharpFunctions.RegisterFunction
ClearRegisteredFunction = CS.LuaCSharp.CSharpFunctions.ClearRegisteredFunctions


-- ------------------------------------------------------
--- globals
g_testint = 50
g_red = {
    r = 1,
    g = 0.5,
    b = 0,
    a = 1
}


-- ------------------------------------------------------
--- locals
local output_file_prefix = "lua_common"


-- ------------------------------------------------------
--- 打印 package.loaded 和 _G 的内容到文件
function PrintGlobal()
    print("PrintGlobal started ")
    local file, err = io.open(output_file_prefix .. "_package_loaded.txt", "w")
    if not file then
        print("Error opening file: " .. err)
        return
    end

    for k, v in pairs(package.loaded) do
        file:write(k,'=',tostring(v),'\n')
    end
	file:close()

    local file, err = io.open(output_file_prefix.."_global.txt", "w")
    if not file then
        print("Error opening file: " .. err)
        return
    end
    for k, v in pairs(_G) do
        file:write(k,'=',tostring(v),'\n')
    end
	file:close()
    print("PrintGlobal finished ")
end

function PrintCS()
    print("PrintCS started ")
    local file, err = io.open(output_file_prefix .. "_cs.txt", "w")
    if not file then
        print("Error opening file: " .. err)
        return
    end

    for k, v in pairs(_G.CS.LuaCSharp.CSharpFunctions) do
        file:write(tostring(k),'=',tostring(v),'\n')
    end
	file:close()
    print("PrintCS finished ")
end



