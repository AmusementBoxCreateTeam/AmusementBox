<?php echo $this->load->view('login/header') ?>
<div class="container">
    <div class="col-md-5 col-md-offset-4">
        <?php echo validation_errors(); ?>
        <?php echo !empty($error)?$error:''; ?>
    </div>
    <div class="row">
        <div class="col-md-4 col-md-offset-4">
            <div class="panel panel-default">
                <div class="panel-heading">
                    <span class="glyphicon glyphicon-lock"></span> ログイン
                </div>
                <div class="panel-body">
                    <?php echo form_open('index/try_login', array("class" => "form-horizontal", "role" => "form")) ?>
                    <div class="form-group">
                        <label for="inputEmail3" class="col-sm-3 control-label">
                            Login ID</label>
                        <div class="col-sm-9">
                            <input name="id" type="text" class="form-control" id="inputEmail3" placeholder="ID" required>
                        </div>
                    </div>
                    <div class="form-group">
                        <label for="inputPassword3" class="col-sm-3 control-label">
                            Password</label>
                        <div class="col-sm-9">
                            <input name="password" type="password" class="form-control" id="inputPassword3" placeholder="Password" required>
                        </div>
                    </div>
                    <div class="form-group last">
                        <div class="col-sm-offset-3 col-sm-9">
                            <button type="submit" class="btn btn-success btn-sm">
                                ログイン</button>
                        </div>
                    </div>
                    </form>
                </div>
                <div class="panel-footer">
                    <a href="<?php echo base_url() . "index.php/index/forgot" ?>">パスワードを忘れた方はこちら&nbsp;&gt;&gt;</a></div>
            </div>
        </div>
    </div>
</div>
<?php
echo $this->load->view('login/footer');
